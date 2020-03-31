namespace ToDoListMaterialDesign.Models.Interface
{
    /// <summary>
    /// 処理依頼
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// 処理依頼パラメーター
        /// </summary>
        object Parameter { get; set; }
    }
}
