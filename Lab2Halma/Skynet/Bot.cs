
namespace Lab2Halma.Skynet
{
    internal abstract class Bot : Player
    {
        /*
        Not to tak:
        - mogę przekazywać i zagregować ruchy w kolekcji / kolejce i na jej podstawie odnośić się do wezłów, czyli stanów gry ,
            bez konieczności rozwijania całego drzewa
                * analiza błędów?
                * powrót wsteczny ddo innych węzłów?? 
                * generalnie do przetwarzania i tworzenia drzewa mogę wykorzystac instancję halmy 
                * co do drzewa:
                        > trzymam jedną instancję rozwiniętą w chuj ale raczej nie 
                        > albo tworzyć i rozwijac tylko drzewo do określonej głębokości jak podane w zadaniu 
                        > czy historia ruchów wtedy mi się na coś zda? być może do szybszego ładowania stanu 
                            za pomocą Halma.LoadState i GetAllPossibleMoves(gameboardState)
                            UWAGA DODATKOWA GetAllPossibleMoves(gameboardState) -> returns HashSet<Moves>
                * heurystyki / funkcje nagrody czyli "JAK SKACZESZ TO JEST W CHUJ DOBRZE"
                        > generalny ruch jeżeli nie jest skokiem (albo każdy) ma się przesuwać się najbliżej wolnego pola w bazie przeciwnika,
                            a jak nie ma wolnych pól to do przeciwnego rogu 
                        > mogę konsolidować ruchy do tych które skaczą najbliżej pola przeciwnika
                        > mogę prioretyzować ruchy skaczące od przesuwalnych 
                        > mogę prioretyzować sytuacje tworzące przerwy pomiędzy pionami do skakania w przyszłych ruchach
        - dzieki temu od razu mam historię ruchów oraz swoją i mogę tworzyć np dane historyczne aby prześledzić partię 
        - czyli jak taką historię dupnę do bazy danych to mogę odtwarzać historyczne zagrania 
        - UWAGA sprawdzaj np czy gracz MOZE wykona jakikolwiek ruch !!!
        */
        protected Bot(int id) : base(id)
        {
            InnerHalma = new Halma();
            Strategies = new List<Heuristic>();
        }

        protected static readonly int VALUE_TO_NORMALIZE = 100;
        protected static readonly int MAX_DISTANCE_MANHATTAN = Halma.BOARD_SIZE * 2;

        public GameStateTree? GameStateTree { get; set; }

        public Halma? InnerHalma { get; set; }
        
        public delegate int Heuristic(Halma.Move move, int pawn, int[,]? gameBoardState = null);

        public List<Heuristic> Strategies { get; set; }

        public void SetStrategies(params Heuristic[] heuristics)
        {
            Strategies.Clear();
            foreach (var item in heuristics)
                Strategies.Add(item);
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        internal override Halma.Move? ChooseMove(int[,] gameBoardState, ref Dictionary<char, int> alphabet, out Halma.Move? invalidMove, HashSet<Halma.Move>? possibleMoves = null)
        {
            return base.ChooseMove(gameBoardState, ref alphabet, out invalidMove, possibleMoves);
        }
    }

    public static class Calculations
    {
        public static int PercantageWithMultiplier(this int caller, int baseValue, int multiplier = 1)
        {
            caller = Math.Abs(caller);

            double percentage = (double)caller / baseValue * 100.0 * multiplier;

            return (int) Math.Round(percentage);
        }
    }
}
