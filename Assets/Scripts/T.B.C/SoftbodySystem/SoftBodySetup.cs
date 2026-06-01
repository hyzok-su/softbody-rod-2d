using System.Collections.Generic;
using UnityEngine;
using SoftBodyLibrary;
using UnityEngine.InputSystem;

public class SoftBodySetup : MonoBehaviour
{

    public List<Rod> rods = new List<Rod>();
    public List<Particle> particles = new List<Particle>();
    public List<Spring> springs = new List<Spring>();
    public List<Bend> bends = new List<Bend>();
    public List<Anchor> anchors = new List<Anchor>();

    public (Material Material, float Width)[] renderSprings = null;

    [Header("SystemParameters")]
    public float gravity = 9.8f;
    public float deltaTimeMilisecond = 10f;
    public float drag = 0.99f;
    public float maxSpeed = 2.0f;
    public float friction = 0.3f;
    public float restitution = 0.3f;

    public SoftBodySystem system;

    // Call from a UI button

    //Rods
    public void RunLinesToRods()
    {
        rods.Clear();
        particles.Clear();
        springs.Clear();
        bends.Clear();
        anchors.Clear();

        renderSprings = new (Material Material, float Width)[transform.childCount];

        // Loop through all children that have a LinesToRods component
        int index = 0;
        foreach (Transform child in transform)
        {
            LinesToRods linesToRods = child.GetComponent<LinesToRods>();
            renderSprings[index] = linesToRods.renderSpring;

            if (linesToRods != null)
            {
                // Call the method, passing the shared lists
                linesToRods.ConvertLinesToRods(
                    rods,
                    particles,
                    springs,
                    bends,
                    anchors,
                    index
                );
            }
            index++;
        }

        Debug.Log(
            "Lines converted to rods: " +
            particles.Count + " particles, " +
            springs.Count + " springs, " +
            bends.Count + " bends."
        );

        // Convert to arrays
        Rod[] rodArray = rods.ToArray();
        Particle[] particleArray = particles.ToArray();
        Spring[] springArray = springs.ToArray();
        Bend[] bendArray = bends.ToArray();
        Anchor[] anchorArray = anchors.ToArray();

        // Create system
        system = new SoftBodySystem(
            rodArray,
            particleArray,
            springArray,
            bendArray,
            anchorArray,
            new Vector2(0, -gravity),
            deltaTimeMilisecond / 1000f,
            drag,
            maxSpeed,
            friction,
            restitution
        );
    }


    // Optional: press R key to run
    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RunLinesToRods();
        }
    }
}
