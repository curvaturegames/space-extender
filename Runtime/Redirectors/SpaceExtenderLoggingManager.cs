using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.IO;
using CurvatureGames.SpaceExtender;

namespace CurvatureGames.SpaceExtender
{
    public class SpaceExtenderLoggingManager : MonoBehaviour
    {
        

        private static SpaceExtenderLoggingManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                if (LoggingEnabled == true)
                {
                    using (StreamWriter writer = new StreamWriter(Getpath()))
                    {
                        Debug.Log("Hallo");
                        //First Line in File
                        writer.WriteLine("RotatorID,Rotation Duration,Added up Rotation Degree");
                    }
                }
            }
            else
            {
                Debug.LogError("There is more than one SpaceExtenderLoggingManager in this scene. The one on " + gameObject.name + " is not used.");
                Destroy(this);
            }
        }

        public static SpaceExtenderLoggingManager Instance { 
            get 
            {
                if (instance == null) {
                    var gameobject = new GameObject("SpaceExtenderLoggingManager");
                    instance = gameobject.AddComponent<SpaceExtenderLoggingManager>();
                    instance.LoggingEnabled = false;
                }
                return instance;
            } 
            set
            {
                instance = value;
            }
        }

        public bool LoggingEnabled = true;

        public void LogData(string name, float totaltime, float totalangle)
        {
            if (LoggingEnabled == false)
            {
                return;
            }
            //Set up the StreamWriter
            using (StreamWriter writer = new StreamWriter(Getpath(),append:true))
            {
                //write Data
                writer.WriteLine(name + "," + totaltime.ToString().Replace(",", ".") + "," + totalangle.ToString().Replace(",", "."));
                writer.Flush();
            }
        }


        private string Getpath()
        {
            return Application.dataPath + "/SpaceExtenderLogging" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm") + ".csv";
        }

    }
}