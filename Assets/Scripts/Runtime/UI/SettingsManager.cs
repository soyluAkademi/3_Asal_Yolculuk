using System.Collections.Generic;
using PayeGames.AsalYolculuk.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Ayarlar panelinin kontrolcüsü. Eski toggle butonlu ses sistemi kaldırıldı;
    /// yerine her ses kanalı için yatayda sürüklenen bir daire tutamağı kullanılır.
    /// Panel yalnızca arayüzdür: seviyenin sahibi <see cref="SoundManager"/>'dır.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Tutamaklar (circle)")]
        [Tooltip("Intro/arka plan müziğinin seviyesini süren daire")]
        [SerializeField] private RectTransform musicCircle;
        [Tooltip("Mermi dışındaki tüm efektlerin seviyesini süren daire")]
        [SerializeField] private RectTransform effectCircle;
        [Tooltip("Geminin ateş sesinin seviyesini süren daire")]
        [SerializeField] private RectTransform bulletCircle;

        [Header("Ray Sınırları (anchoredPosition.x)")]
        [Tooltip("Sol uç: ses = 0")]
        [SerializeField] private float minX = -173f;
        [Tooltip("Sağ uç: ses = 1")]
        [SerializeField] private float maxX = 190f;

        private readonly List<HandleBinding> handles = new List<HandleBinding>(3);
        private Camera uiCamera;

        private void Awake()
        {
            CacheUiCamera();

            BindHandle(musicCircle,  SoundChannel.Music);
            BindHandle(effectCircle, SoundChannel.Effect);
            BindHandle(bulletCircle, SoundChannel.Bullet);
        }

        private void OnDisable()
        {
            // Panel kapanırken (ör. pause menüsünden çıkış) son değer garantiye alınır;
            // kullanıcı tutamağı bırakmadan sahne değiştirebilir.
            for (int index = 0; index < handles.Count; index++)
            {
                SaveVolume(handles[index].channel);
            }
        }

        private void OnEnable()
        {
            // Panel her açıldığında tutamaklar güncel seviyeye çekilir; ayar başka
            // bir sahnede değişmiş olabilir.
            for (int index = 0; index < handles.Count; index++)
            {
                HandleBinding binding = handles[index];
                MoveHandleToVolume(binding, SoundManager.ReadStoredVolume(binding.channel));
            }
        }

        private void BindHandle(RectTransform circle, SoundChannel channel)
        {
            if (circle == null)
            {
                Debug.LogWarning($"[SettingsManager] {channel} tutamağı atanmadı.", this);
                return;
            }

            HandleBinding binding = new HandleBinding
            {
                circle = circle,
                // Tutamağın x'i parent'ının local uzayında ölçüldüğü için sürükleme
                // hesabı da aynı rect üzerinden yapılır.
                track = circle.parent as RectTransform,
                channel = channel,
                startY = circle.anchoredPosition.y
            };

            handles.Add(binding);
            HookDragEvents(binding);
        }

        private void OnHandleDrag(HandleBinding binding, BaseEventData eventData)
        {
            if (eventData is not PointerEventData pointerData) return;
            if (binding.track == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    binding.track, pointerData.position, uiCamera, out Vector2 localPoint))
            {
                return;
            }

            float clampedX = Mathf.Clamp(localPoint.x, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));

            // Y sabit kalır: tutamak sadece yatay eksende hareket eder.
            binding.circle.anchoredPosition = new Vector2(clampedX, binding.startY);

            ApplyVolume(binding.channel, Mathf.InverseLerp(minX, maxX, clampedX));
        }

        private void OnHandleDragEnd(HandleBinding binding, BaseEventData eventData)
        {
            SaveVolume(binding.channel);
        }

        /// <summary>
        /// Ayarı diske yazar. SoundManager sahnede yoksa da kayıt düşülür; aksi
        /// halde kullanıcının yaptığı ayar sessizce kaybolurdu.
        /// </summary>
        private static void SaveVolume(SoundChannel channel)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SaveVolume(channel);
                return;
            }

            PlayerPrefs.SetFloat(
                SoundManager.GetVolumePrefKey(channel),
                SoundManager.ReadStoredVolume(channel));
            PlayerPrefs.Save();
        }

        private void MoveHandleToVolume(HandleBinding binding, float volume)
        {
            binding.circle.anchoredPosition =
                new Vector2(Mathf.Lerp(minX, maxX, volume), binding.startY);
        }

        /// <summary>
        /// Seviyeyi SoundManager'a yazar; ses durumunun tek sahibi odur, bu panel
        /// yalnızca onu süren arayüzdür.
        /// </summary>
        private static void ApplyVolume(SoundChannel channel, float volume)
        {
            if (SoundManager.Instance == null) return;
            SoundManager.Instance.SetVolume(channel, volume);
        }

        private void CacheUiCamera()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Canvas rootCanvas = canvas.rootCanvas;
            uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;
        }

        /// <summary>
        /// Sürükleme olaylarını kod tarafından bağlar; böylece Inspector'da tek
        /// yapılması gereken daire referanslarını atamaktır.
        /// </summary>
        private void HookDragEvents(HandleBinding binding)
        {
            if (!binding.circle.TryGetComponent(out EventTrigger trigger))
            {
                trigger = binding.circle.gameObject.AddComponent<EventTrigger>();
            }

            AddTriggerEntry(trigger, EventTriggerType.PointerDown, data => OnHandleDrag(binding, data));
            AddTriggerEntry(trigger, EventTriggerType.Drag,        data => OnHandleDrag(binding, data));
            AddTriggerEntry(trigger, EventTriggerType.EndDrag,     data => OnHandleDragEnd(binding, data));
            AddTriggerEntry(trigger, EventTriggerType.PointerUp,   data => OnHandleDragEnd(binding, data));
        }

        private static void AddTriggerEntry(
            EventTrigger trigger,
            EventTriggerType type,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        /// <summary>Bir daire tutamağını kendi ses kanalına bağlayan runtime kaydı.</summary>
        private class HandleBinding
        {
            public RectTransform circle;
            public RectTransform track;
            public SoundChannel channel;
            public float startY;
        }
    }
}
