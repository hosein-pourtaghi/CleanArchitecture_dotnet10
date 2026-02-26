using SharedKernel;

namespace Domain.Todos;

public class TodoItems : Entity
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public Guid TodoId{ get; set; }


    #region Navigations 
    public virtual Todo Todo{ get; set; }
    #endregion


}
