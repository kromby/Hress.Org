﻿using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class PartyUser : EntityBase<int>
{
    public PartyUser(int userId, int parentId, string username, int photoId, string roleName)
    {
        ID = userId;
        Name = username;
        User = new()
        {
            ID = userId,
            Username = username,
            ProfilePhotoId = photoId
        };
        Role = roleName;
        ParentID = parentId;
    }

    public UserBasicEntity User { get; set; }

    public int ParentID { get; set; }
    public string Role { get; set; }
}
