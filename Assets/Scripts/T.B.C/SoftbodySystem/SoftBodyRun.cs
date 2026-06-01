using System.Collections.Generic;
using UnityEngine;
using SoftBodyLibrary;

public class SoftBodyRunner : MonoBehaviour
{

    [Header("Setup Reference")]
    public SoftBodySetup setup;
    private (Material Material, float Width)[] renderSprings = null;

    public int iteration = 25;

    private SoftBodySystem system;
    private List<LineRenderer> springLines = new List<LineRenderer>();
    private List<GameObject> objBuffer = new List<GameObject> ();
    
    
    void Update()
    {
        // Press Run button (or R key) to start
        if (setup != null && setup.system != null && setup.system != system)
        {
            // Grab the system reference from setup
            system = setup.system;
            renderSprings = setup.renderSprings;

            InitializeVisualization();
        }

        if (system != null)
        {
            // Step simulation

            for (int i = 0; i < iteration; i++)
            {
                system.Step();
            }

            // Update spring LineRenderers
            UpdateSprings();
        }
    }

    void InitializeVisualization()
    {
        springLines.Clear();

        for (int i = 0; i < objBuffer.Count; i++)
        {
            if (objBuffer[i] != null)
                Destroy(objBuffer[i]);
        }
        objBuffer.Clear();

        Spring[] springsArray = system.springs;

        for (int i = 0; i < springsArray.Length; i++)
        {
            GameObject go = new GameObject("SpringLine_" + i);
            go.transform.parent = transform;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;

            ref Spring s = ref springsArray[i];
            lr.material = renderSprings[s.RenderIndex].Material;
            lr.startWidth = lr.endWidth = renderSprings[s.RenderIndex].Width;
            lr.useWorldSpace = true;
            //lr.numCapVertices = 4;
            //lr.numCornerVertices = 4;

            springLines.Add(lr);
            objBuffer.Add(go);

            if (!springsArray[i].JointMark)
                go.SetActive(false);
        }
    }

    void UpdateSprings()
    {
        Spring[] springsArray = system.springs;
        Particle[] particles = system.particles;

        for (int i = 0; i < springsArray.Length; i++)
        {
            ref Spring s = ref springsArray[i];
            LineRenderer lr = springLines[i];

            lr.SetPosition(0, particles[s.IndexA].Position);
            lr.SetPosition(1, particles[s.IndexB].Position);
        }
    }

    public void HideAllObj()
    {
        foreach (var obj in objBuffer)
        {
            if (obj != null)
                obj.SetActive(false); // hide the whole GameObject
        }
    }
}
