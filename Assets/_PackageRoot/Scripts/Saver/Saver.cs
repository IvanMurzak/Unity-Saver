using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UniRx;
using Extensions.Saver.Utils;

namespace Extensions.Saver
{
    public class Saver<T> : ISavable, ILoadable<T>
    {
        #region static
        public static void Init()
        {
            if (PersistantDataPath != null)
                return;

            EncryptionUtils.Init();

            PersistantDataPath = SaverInitializer.Config.Location;

            if (SaverInitializer.Config.debug)
                Debug.Log($"Saver.Init {PersistantDataPath}");
        }
        private static string PersistantDataPath = null;

        public static readonly TaskFactory factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));

        public static string FullPath(string path) => $"{PersistantDataPath}/{path}";
        public static string FullPath(string path, string fileName) => $"{PersistantDataPath}/{path}/{fileName}";

        private static bool IsFileExists(string path, string fileName)
        {
            var fullPath = FullPath(path, fileName);
            return File.Exists(fullPath);
        }

        public static void Save(T data, string path, string fileName)
        {
            Save(data, FullPath(path, fileName));
        }
        public static void Save(T data, string fullPath)
        {
            if (SaverInitializer.Config.debug)
                Debug.Log($"Save:{fullPath}");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                var bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary);
                File.WriteAllBytes(fullPath, EncryptionUtils.Encrypt(bytes));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        public static T Load(string path, string fileName)
        {
            return Load(FullPath(path, fileName));
        }
        public static T Load(string fullPath)
        {
            if (SaverInitializer.Config.debug)
                Debug.Log($"Load:{fullPath}");
            T data;
            if (!File.Exists(fullPath))
            {
                data = Activator.CreateInstance<T>();
                return data;
            }
            byte[] bytes = File.ReadAllBytes(fullPath);
            try
            {
                data = SerializationUtility.DeserializeValue<T>(EncryptionUtils.Decrypt(bytes), DataFormat.Binary);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                data = default(T);
            }
            return data;
        }
        public static void Delete(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        public static void DeleteAllSaves()
        {
            if (SaverInitializer.Config.debug)
                Debug.Log($"DeleteAllSaves at path: {PersistantDataPath}");
            if (Directory.Exists(PersistantDataPath))
                Directory.Delete(PersistantDataPath, true);
        }
        private static void CheckSerializableAttribute()
        {
            var serializable = typeof(T).IsDefined(typeof(System.SerializableAttribute), false);
            if (!serializable) throw new IOException($"{typeof(T).Name} isn't Serializable");
        }
        #endregion

        CompositeDisposable disposable;
        CompositeDisposable Disposable
        {
            get
            {
                if (disposable == null) 
                    disposable = new CompositeDisposable();
                return disposable;
            }
        }

        [SerializeField, ShowInInspector, ReadOnly] private string path;
        [SerializeField, ShowInInspector, ReadOnly] private string fileName;

        public bool Loaded { get; private set; } = false;

        [BoxGroup("Data", false), NonSerialized, OdinSerialize, ReadOnly] public T data;
        [BoxGroup("Data", false), NonSerialized, OdinSerialize] private T _defaultData;
        public T DefaultData
        {
            private set => _defaultData = value;
            get
            {
                if (_defaultData != null && _defaultData is ICloneable)
                {
                    return (T)((ICloneable)_defaultData).Clone();
                }
                else
                {
                    return _defaultData;
                }
            }
        }

        [ShowInInspector, ReadOnly] private string fullPath => FullPath(path, fileName);

        public Saver()
        {
            Init();
            DefaultData = Activator.CreateInstance<T>();
        }
        public Saver(string path, string fileName) : this()
        {
            UpdatePath(path, fileName);
        }
        public Saver(string path, string fileName, T defaultData) : this(path, fileName)
        {
            this.DefaultData = defaultData;
        }
        [BoxGroup("Data", false), ButtonGroup("Data/Buttons2"), Button(ButtonSizes.Medium)]
        public void CopyFromDefault()
        {
            data = DefaultData.Copy();
        }
        [BoxGroup("Data", false), ButtonGroup("Data/Buttons2"), Button(ButtonSizes.Medium), GUIColor(1, .6f, .4f, 1)]
        public void ClearDefaultData()
        {
            _defaultData = default(T);
        }
        [BoxGroup("Data", false), ButtonGroup("Data/Buttons1"), Button(ButtonSizes.Medium)]
        public void ClearData()
        {
            data = default(T);
        }
#if UNITY_EDITOR
        [BoxGroup("Data", false), ButtonGroup("Data/Buttons1"), Button(ButtonSizes.Medium), GUIColor(1, .3f, .3f, 1)]
        private void DeleteSave()
        {
            var fullPath = FullPath(path, fileName);
            UnityEditor.FileUtil.DeleteFileOrDirectory(fullPath);
        }
#endif
        public void UpdatePath(string path, string fileName)
        {
            Init();
            this.path = path;
            this.fileName = fileName;
        }
        public bool IsFileExists()
        {
            return IsFileExists(path, fileName);
        }

        public async Task Save(Action onComplete = null)
        {
            Init();
            Disposable.Clear();
            await factory.StartNew(() => Save(data, path, fileName));
            onComplete?.Invoke();
        }
        public void SaveDelayed(Action onComplete = null) => SaveDelayed(TimeSpan.FromSeconds(1), onComplete);
        public void SaveDelayed(TimeSpan delay, Action onComplete = null)
        {
            //Observable.Timer(delay, Scheduler.ThreadPool)
            //  .First()
            //  .Subscribe(async x => await Save(onComplete))
            //  .AddTo(Disposable);

            // This more complex code supports pure NuGet ReactiveProperty package.
            // UniRx.ObservableExtensions.Subscribe<T>
            // System.ObservableExtensions.Subscribe<T>
            UniRx.ObservableExtensions.Subscribe
            (
                Observable.Timer(delay, Scheduler.ThreadPool)
                    .First(),
                async x => await Save(onComplete)
            ).AddTo(Disposable);

        }
        public T Load()
        {
            Init();
            if (DefaultData == null)
            {
                if (SaverInitializer.Config.debug)
                    Debug.LogError($"Default data is null. path={FullPath(path, fileName)}");
            }
            if (IsFileExists())
            {
                data = Load(path, fileName);
                if (data == null)
                {
                    if (SaverInitializer.Config.debug)
                        Debug.LogError($"Loaded data is null. path={FullPath(path, fileName)}");
                    data = DefaultData.Copy();
                }
            }
            else
            {
                if (SaverInitializer.Config.debug)
                    Debug.Log($"Loading default data. path={FullPath(path, fileName)}");
                data = DefaultData.Copy();
            }
            Loaded = true;
            return data;
        }
        public async Task<T> LoadAsync(Action<T> onComplete = null)
        {
            Init();
            var result = await factory.StartNew(Load);
            onComplete?.Invoke(result);
            return result;
        }
    }

    public interface ISavable
    {
        [Button, HorizontalGroup("Save Buttons")]
        Task Save(Action onComplete = null);
    }
    public interface ILoadable<T>
    {
        [Button, HorizontalGroup("Save Buttons")]
        T Load();
    }
}
