using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System;

namespace Extensions.Saver
{
    public abstract class SaverScriptableObject<T> : SerializedScriptableObject, ISavable, ILoadable<T>
    {
        [FoldoutGroup("Saving"), BoxGroup("Saving/Data", false), SerializeField, ShowInInspector, ShowIf("HasSaver")]
        private                 Saver<T>    saver                   = null;

        protected               T           Data                    { get; set; }
        protected               T           DefaultData             => saver.DefaultData;

        protected   abstract    string      SaverPath               { get; }
        protected   abstract    string      SaverFileName           { get; }

        public                  bool        HasSaver                => saver != null;


        [HorizontalGroup("Managering Data"), Button(ButtonSizes.Medium)]
        public                  T           Load()
        {
            if (saver == null)
            {
                if (SaverInitializer.Config.debug)
                    Debug.Log($"Saver had not been initialized. Doing it automaticly.");
                saver = new Saver<T>(SaverPath, SaverFileName);
            }
            Data = PrepareData(saver.Load());
            OnDataLoaded(Data);
            return Data;
        }
        public                  Task        LoadAsync()             => saver.LoadAsync(task => OnDataLoaded(Data = PrepareData(saver.data)));
        [HorizontalGroup("Managering Data"), Button(ButtonSizes.Medium)]
        public      virtual     async Task  Save(Action onComplete = null)
        {
            saver.data = OnDataSave(Data);
            await saver.Save(onComplete);
        }
        public      virtual     void        SaveDelayed(TimeSpan delay, Action onComplete = null)
        {
            saver.data = OnDataSave(Data);
            saver.SaveDelayed(delay, onComplete);
        }

        protected   virtual     void        OnEnable()
        {
            EncryptionUtils.Init();
            Data = Load();
        }
        protected   virtual     T           PrepareData(T data)     => data;
        protected   abstract    void        OnDataLoaded(T data);
        protected   virtual     T           OnDataSave(T data)      => data;


        [FoldoutGroup("Saving"), Button(ButtonSizes.Medium), GUIColor(1, .6f, .4f, 1), ShowIf("HasSaver")]
        private                 void        UpdatePath()            => saver.UpdatePath(SaverPath, SaverFileName);
        [FoldoutGroup("Saving"), Button(ButtonSizes.Large), GUIColor(.6f, 1, .4f, 1), HideIf("HasSaver")]
        private                 void        InitSaver()
        {
            if (saver == null)
            {
                saver = new Saver<T>(SaverPath, SaverFileName);
            }
            else
            {
                saver.UpdatePath(SaverPath, SaverFileName);
            }
        }
    }
}
