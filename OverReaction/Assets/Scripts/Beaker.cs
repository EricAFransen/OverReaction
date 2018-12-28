using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Beaker : MonoBehaviour {
    private List<GameObject> compounds;
    private List<Rigidbody> compoundsRigidBody;
    private List<Renderer> compoundsRenderer;
    private int particles = 0;// = 700;
    private Color[] colorList = { Color.red, Color.blue, Color.yellow, Color.green, Color.black};
    // Use this for initialization
    public Beaker(int[] species) {
        genBeaker();
        int sum = 0;
        for (int s = 0; s < species.Length; s++)
            sum += species[s];
        particles = sum;
        //Debug.Log(particles + " particles");
        System.Random r = new System.Random();
        
        compounds = new List<GameObject>();
        compoundsRigidBody = new List<Rigidbody>();
        compoundsRenderer = new List<Renderer>();
        int i = 0;
        for(int j=0; j<species.Length; j++)
        {
            for (int k = 0; k < species[j]; k++)
            {
                compounds.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                compounds[i].transform.position = new Vector3(0, 0, 0);
                compounds[i].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                compounds[i].transform.position = new Vector3(r.Next(-1, 1), r.Next(-2, 2), r.Next(-1, 1));

                compoundsRigidBody.Add(compounds[i].AddComponent<Rigidbody>());
                compoundsRigidBody[i].useGravity = false;
                compoundsRigidBody[i].velocity = new Vector3(r.Next(-5, 5), r.Next(-5, 5), r.Next(-5, 5)).normalized;

                compoundsRenderer.Add(compounds[i].GetComponent<Renderer>());
                compoundsRenderer[i].material.shader = Shader.Find("_Color");
                compoundsRenderer[i].material.SetColor("_Color", colorList[j]);
                compoundsRenderer[i].material.shader = Shader.Find("Specular");
                compoundsRenderer[i].material.SetColor("Specular", colorList[j]);
                i++;
            }
        }
        
	}
	
	// Update is called once per frame
	public void Update () {
        int rate = 3;
        for (int i = 0; i < particles; i++)
        {
            Vector3 t = compoundsRigidBody[i].velocity.normalized;
            compoundsRigidBody[i].velocity = new Vector3(t.x * rate, t.y * rate, t.z * rate);

            Vector3 p = compounds[i].transform.position;
            if((p.x > 5) || (p.x < -5) || (p.y > 5) || (p.y < -5) || (p.z > 5) || (p.z < -5))
            {
                compounds[i].transform.position = new Vector3(0, 0, 0);
            }
        }  
    }

    //Alter later to dynamicly create beaker and coliders
    private void genBeaker()
    {
        //GameObject beaker = (GameObject)Instantiate(Resources.Load("beaker"));

        /*
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.transform.localScale = new Vector3(10, 10, 0.1f);
        back.transform.position = new Vector3(0, 0, -5);
        Rigidbody backR = back.AddComponent<Rigidbody>();
        backR.useGravity = false;
        backR.isKinematic = true;

        GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
        front.transform.localScale = new Vector3(10, 10, 0.1f);
        front.transform.position = new Vector3(0, 0, 5);
        Rigidbody frontR = front.AddComponent<Rigidbody>();
        frontR.useGravity = false;
        frontR.isKinematic = true;
        front.GetComponent<MeshRenderer>().enabled = false;

        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        left.transform.localScale = new Vector3(0.1f, 10, 10);
        left.transform.position = new Vector3(-5, 0, 0);
        Rigidbody leftR = left.AddComponent<Rigidbody>();
        leftR.useGravity = false;
        leftR.isKinematic = true;

        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        right.transform.localScale = new Vector3(0.1f, 10, 10);
        right.transform.position = new Vector3(5, 0, 0);
        Rigidbody rightR = right.AddComponent<Rigidbody>();
        rightR.useGravity = false;
        rightR.isKinematic = true;

        GameObject up = GameObject.CreatePrimitive(PrimitiveType.Cube);
        up.transform.localScale = new Vector3(10, 0.1f, 10);
        up.transform.position = new Vector3(0, 5, 0);
        Rigidbody upR = up.AddComponent<Rigidbody>();
        upR.useGravity = false;
        upR.isKinematic = true;

        GameObject down = GameObject.CreatePrimitive(PrimitiveType.Cube);
        down.transform.localScale = new Vector3(10, 0.1f, 10);
        down.transform.position = new Vector3(0, -5, 0);
        Rigidbody downR = down.AddComponent<Rigidbody>();
        downR.useGravity = false;
        downR.isKinematic = true;
        */
    }
    
    public void updateColor(int[] species)
    {
        int sum = 0;
        for (int s = 0; s < species.Length; s++)
            sum += species[s];

        if (sum != particles)
        {
            System.Random r = new System.Random();
            if (sum > particles)
            {
                for (int t = 0; t < (sum - particles); t++)
                {
                    compounds.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                    compounds[particles + t].transform.position = new Vector3(0, 0, 0);
                    compounds[particles + t].transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                    compounds[particles + t].transform.position = new Vector3(r.Next(-1, 1), r.Next(-2, 2), r.Next(-1, 1));

                    compoundsRigidBody.Add(compounds[particles + t].AddComponent<Rigidbody>());
                    compoundsRigidBody[particles + t].useGravity = false;
                    compoundsRigidBody[particles + t].velocity = new Vector3(r.Next(-5, 5), r.Next(-5, 5), r.Next(-5, 5)).normalized;

                    compoundsRenderer.Add(compounds[particles + t].GetComponent<Renderer>());
                }


            }
            else
            {
                for (int t = 0; t < (particles - sum); t++)
                {
                    compounds.RemoveAt(compounds.Count - 1);

                    compoundsRigidBody.RemoveAt(compoundsRigidBody.Count - 1);

                    compoundsRenderer.RemoveAt(compoundsRenderer.Count - 1);
                }
            }
            particles = sum;
        }

        int i = 0;
        for (int j = 0; j < species.Length; j++)
        {
            for (int k = 0; k < species[j]; k++)
            {
                compoundsRenderer[i].material.shader = Shader.Find("_Color");
                compoundsRenderer[i].material.SetColor("_Color", colorList[j]);
                compoundsRenderer[i].material.shader = Shader.Find("Specular");
                compoundsRenderer[i].material.SetColor("Specular", colorList[j]);
                i++;
            }
        }
    }
}
