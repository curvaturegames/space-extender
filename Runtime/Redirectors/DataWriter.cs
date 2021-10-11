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
    public class DataWriter : MonoBehaviour
    {

        void Start()
        {


        }

        void Update()
        {



        }


        public void logData(string name, float totaltime, float totalangle)
        {
            //Set up the StreamWriter
            StreamWriter writer = new StreamWriter(Getpath());

            //First Line in File
            writer.WriteLine("RotatorID,Rotation Duration,Added up Rotation Degree");

            //write Data
            writer.WriteLine(name + "," + totaltime.ToString().Replace(",",".") + "," + totalangle.ToString().Replace(",", "."));
            writer.Flush();

            //Closes Stream
            writer.Close();
        }


        private string Getpath()
        {
            return Application.dataPath + "/Data" + DateTime.Now.ToString("MM-dd-yyyy_HH-mm") + ".csv";
        }

    }
}