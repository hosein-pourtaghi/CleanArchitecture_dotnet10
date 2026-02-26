using SharedKernel;

namespace Domain.Todos;

public class Todo : Entity
{
    public Guid Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public int Priority { get; set; }
    public DateTime Date { get; set; }



    #region Navigations 
    public virtual ICollection<TodoItems> TodoItems { get; set; }
    #endregion


}
