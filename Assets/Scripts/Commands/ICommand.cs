using System.Threading.Tasks;

public interface ICommand
{
    public virtual async Task Execute() { }
}