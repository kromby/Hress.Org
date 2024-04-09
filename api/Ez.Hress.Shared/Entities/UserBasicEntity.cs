namespace Ez.Hress.Shared.Entities
{
    public class UserBasicEntity : EntityBase<int>
    {
        public string? Username { get; set; }

        public int ProfilePhotoId { private get; set; }

        public string Href
        {
            get => string.Format("/api/users/{0}", ID);
        }

        public HrefEntity? ProfilePhoto
        {
            get
            {
                if (ProfilePhotoId > 0)
                {
                    return new HrefEntity()
                    {
                        Href = string.Format("/api/images/{0}/content", ProfilePhotoId),
                        ID = ProfilePhotoId
                    };
                }
                return null;
            }
        }
    }
}
