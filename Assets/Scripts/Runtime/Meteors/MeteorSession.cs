namespace PayeGames.AsalYolculuk.Meteors
{
    /// <summary>
    /// Bir ana meteorun parçalanma sürecini takip eder. Ana meteordan doğan tüm
    /// parçalar aynı oturumu paylaşır; puan ancak oturumdaki son dal da yok
    /// edildiğinde verilir. Herhangi bir parça ekrandan çıkarsa oturum iptal olur.
    /// </summary>
    public class MeteorSession
    {
        // Oturum, kök meteorun kendisi tek aktif dal olacak şekilde başlar.
        private int activeBranchCount = 1;

        public MeteorSession(int rootNumber)
        {
            RootNumber = rootNumber;
        }

        public int RootNumber { get; }
        public bool IsCancelled { get; private set; }
        public bool IsCompleted { get; private set; }

        /// <summary>Bir dal parçalandı: dal kapanır, yerine <paramref name="fragmentCount"/> yeni dal açılır.</summary>
        public void RegisterSplit(int fragmentCount)
        {
            activeBranchCount += fragmentCount - 1;
        }

        /// <summary>Bir dal tamamen yok edildi.</summary>
        /// <returns>Oturum bu vuruşla tamamlandıysa (puan verilmeli) true.</returns>
        public bool RegisterBranchDestroyed()
        {
            activeBranchCount--;

            if (IsCancelled) return false;
            if (activeBranchCount > 0) return false;

            IsCompleted = true;
            return true;
        }

        /// <summary>Bir parça ekrandan çıktı; bu meteor tamamlanmamış sayılır.</summary>
        public void Cancel()
        {
            activeBranchCount--;
            IsCancelled = true;
        }
    }
}
