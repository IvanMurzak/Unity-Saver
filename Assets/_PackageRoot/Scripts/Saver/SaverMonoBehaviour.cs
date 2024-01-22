using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Serialization;
using UnityEngine;
using UniRx;

namespace Extensions.Saver
{
    public abstract class SaverMonoBehaviour<T> : SerializedMonoBehaviour
    {
        private             Subject<T>                  onSaveStarted           = new Subject<T>();
        private             Subject<T>                  onDataLoaded            = new Subject<T>();
        private             Subject<T>                  onDataModified          = new Subject<T>();

        [NonSerialized, OdinSerialize, Required, DisableInPrefabAssets]
        [BoxGroup("Saving", false), BoxGroup("Saving/Data", false)/*, ShowIf("HasSaver")*/]
        private             Saver<T>                    saver;

        private             ISaverOnLoadedListener<T>[] loadingDataListeners    = new ISaverOnLoadedListener<T>[0];

        public              bool                        Loaded                  => saver.Loaded;

        public              IObservable<T>              OnSaveStarted           => onSaveStarted;
        public              IObservable<T>              OnDataLoaded            => onDataLoaded;
        public              IObservable<T>              OnDataModified          => onDataModified;

        protected           T                           Data                    { get; set; }
        protected           T                           DefaultData             => saver.DefaultData;
    
        protected abstract  string                      SaverPath               { get; }
        protected abstract  string                      SaverFileName           { get; }

        private             bool                        HasSaver                => saver != null;


        public virtual void PreBuildSetup()
        {
            InitSaver();
        }
        [HorizontalGroup("Managering Data"), Button(ButtonSizes.Medium)]
        public T Load()
        {
            if (saver == null) Debug.LogError("Saver is not initialized!");
            Data = PrepareData(saver.Load());
            NotifyLoadingDataListeners(loadingDataListeners, Data);
            return Data;
        }
        public Task LoadAsync()
        {
            if (saver == null) Debug.LogError("Saver is not initialized!");
            return saver.LoadAsync(task =>
            {
                Data = PrepareData(saver.data);
                NotifyLoadingDataListeners(loadingDataListeners, Data);
            });
        }
        [HorizontalGroup("Managering Data"), Button(ButtonSizes.Medium)]
        public async Task Save(Action onComplete = null)
        {
            if (saver == null) Debug.LogError("Saver is not initialized!");
            saver.data = PrepeareDataBeforeSave(Data);
            await saver.Save(onComplete);
            onSaveStarted.OnNext(Data);
        }
        public void SaveDelayed(TimeSpan delay, Action onComplete = null)
        {
            if (saver == null) Debug.LogError("Saver is not initialized!");
            saver.data = PrepeareDataBeforeSave(Data);
            saver.SaveDelayed(delay, onComplete);
            onSaveStarted.OnNext(Data);
        }


        protected virtual void Start()
        {
            EncryptionUtils.Init();
            PreBuildSetup();

            loadingDataListeners = GetComponents<ISaverOnLoadedListener<T>>();
            var dataModifiers = GetComponents<ISaverDataModifier<T>>();

            ObserveDataModifiers(dataModifiers);
            Load();
        }
        protected virtual T PrepareData(T data)
        {
            return data;
        }
        protected virtual T PrepeareDataBeforeSave(T data) { return data; }
        protected virtual void NotifyLoadingDataListeners(ISaverOnLoadedListener<T>[] dataListeners, T data)
        {
            dataListeners.ForEach(x => x.OnLoaded(data));
            onDataLoaded.OnNext(data);
        }
        protected virtual void ObserveDataModifiers(ISaverDataModifier<T>[] dataModifiers)
        {
            dataModifiers.ForEach(x => x.RegisterDataModifiedListener(OnDataModifiedByModifier));
        }
        protected virtual void OnDataModifiedByModifier(T data)
        {
            Data = data;
            saver.data = Data;
            onDataModified.OnNext(data);
            saver.SaveDelayed();
        }


        [BoxGroup("Saving", false), Button(ButtonSizes.Medium), GUIColor(1, .6f, .4f, 1), ShowIf("HasSaver")]
        private void UpdatePath()
        {
            saver.UpdatePath(SaverPath, SaverFileName);
        }
        [BoxGroup("Saving", false), Button(ButtonSizes.Large), GUIColor(.6f, 1, .4f, 1), HideIf("HasSaver")]
        private void InitSaver()
        {
            if (saver == null)  saver = new Saver<T>(SaverPath, SaverFileName);
            else                saver.UpdatePath(SaverPath, SaverFileName);
            // PrefabUtils.EditorApplyPrefabChanges(this);
        }
        private async void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) await Save();
        }
    #if UNITY_EDITOR
        private async void OnDisable()
        {
            await Save();
        }
    #endif
    }

    public interface ISaverOnLoadedListener<T>
    {
        void OnLoaded(T data);
    }
    public interface ISaverDataModifier<T>
    {
        void RegisterDataModifiedListener(Action<T> onModified);
    }
}
