using System;
using adworks.media_common;

namespace adworks.media_common
{
    [Serializable]
    public class EntityDto
    {
        public EntityDto()
        {
            CreatedOn = DateTimeOffset.Now;
        }
        public Guid Id { get; set; }

        public string? VisibleId {
            get { return Id.ToString("N"); }
            set { Id = new Guid(value); }
        }
        public DateTimeOffset? CreatedOn { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }

        public string? UpdatedBy { get; set; }

        public string? CreatedBy { get; set; }
    }
}
