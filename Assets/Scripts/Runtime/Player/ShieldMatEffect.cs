using UnityEngine;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Kalkan materyalinin Offset vektörünü sürekli kaydırarak akan enerji görüntüsü
    /// üretir. Hologram shader'ında bu, doku ayarlarındaki tiling/offset değil,
    /// shader'ın kendi <c>_Offset</c> vektör özelliğidir. Yalnızca görsel bir
    /// bileşendir; çarpışmayı <see cref="ShieldBarrier"/> yönetir.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class ShieldMatEffect : MonoBehaviour
    {
        [Tooltip("Kaydırılacak vektör özelliğinin adı. Hologram shader'ında: _Offset")]
        [SerializeField] private string offsetPropertyName = "_Offset";
        [Tooltip("Saniyedeki kayma hızı. Eksi değer ters yöne kaydırır")]
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0.2f, 0.1f);

        private Material materialInstance;
        private int offsetPropertyId;
        private Vector2 currentOffset;
        private bool hasOffsetProperty;

        private void Awake()
        {
            Renderer targetRenderer = GetComponent<Renderer>();

            // sharedMaterial DEĞİL: o materyal asset'ini kalıcı olarak değiştirir ve
            // editörde oyun durduğunda offset kaymış olarak kalırdı.
            materialInstance = targetRenderer.material;

            offsetPropertyId = Shader.PropertyToID(offsetPropertyName);
            hasOffsetProperty = materialInstance.HasProperty(offsetPropertyId);

            if (hasOffsetProperty) return;

            Debug.LogWarning($"[ShieldMatEffect] Materyalde '{offsetPropertyName}' " +
                "özelliği yok; kayma çalışmayacak.", this);
        }

        private void OnDestroy()
        {
            // targetRenderer.material bir kopya üretir; sahibi bu bileşendir.
            if (materialInstance == null) return;

            Destroy(materialInstance);
        }

        private void Update()
        {
            if (!hasOffsetProperty) return;

            currentOffset += scrollSpeed * Time.deltaTime;

            // Değer sonsuza büyüyüp hassasiyet kaybetmesin; doku zaten tekrarlanıyor.
            currentOffset.x %= 1f;
            currentOffset.y %= 1f;

            materialInstance.SetVector(offsetPropertyId, currentOffset);
        }
    }
}
