namespace ItemProcessingSystemCore.Models
{
    public class ItemRelation
    {
        public int RelationId { get; set; }
        public int ParentItemId { get; set; }
        public int ChildItemId { get; set; }
    }
}