using UnityEngine;
using System.Collections;
using System;


/*
 * 
{
    "id": 2,
    "email": "tester",
    "password_digest": "111",
    "role": "artist",
    "city": "",
    "created_at": "2016-06-20T06:08:19.000Z",
    "updated_at": "2016-06-20T06:08:19.000Z"
}
 * 
 */
[Serializable]
public class LoginResp : JsonBase
{
    public int id;
    public string email;
    public string password_digest;
    public string role;
    public string city;
    public string created_at;
    public string updated_at;
}
