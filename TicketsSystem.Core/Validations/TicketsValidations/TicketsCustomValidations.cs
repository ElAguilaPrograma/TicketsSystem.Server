namespace TicketsSystem.Core.Validations.TicketsValidations
{
    public interface ITicketsCustomValidations
    {
        bool CorrectPriorityValue(int priorityValue);
        bool CorrectStatusValue(int statusValue);
    }

    public class TicketsCustomValidations : ITicketsCustomValidations
    {
        public TicketsCustomValidations() { }

        public bool CorrectStatusValue(int statusValue)
        {
            if (statusValue <= 0 || statusValue > 5)
                return false;

            return true;
        }

        public bool CorrectPriorityValue(int priorityValue)
        {
            if (priorityValue <= 0 || priorityValue > 4)
                return false;

            return true;
        }

    }
}
