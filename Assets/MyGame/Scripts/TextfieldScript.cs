using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Teacher
{
    public string name;
    public string sprechstunde;
    public string email;
}


public class Room
{
    public List<Teacher> teachers;
}

public class Textfield : MonoBehaviour
{
    public string teacherlinks;
    void Start()
    {

    }

    void Update()
    {
        
    }
}
