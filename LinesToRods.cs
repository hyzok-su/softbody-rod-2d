using System.Collections.Generic;
using UnityEngine;
using SoftBodyLibrary;

public class LinesToRods : MonoBehaviour
{
    // Reference 
    [Header("Reference")]
    public LineDrawer2D lineDrawer;

    public (Material Material, float Width) renderSpring;
   
    private List<Vector2[]> lines = new List<Vector2[]>();
    private List<bool[]> anchorTags = new List<bool[]>();

    [Header("Particle")]
    public float mass = 1f;

    [Header("Spring")]
    public float springStiffness = 100f;
    public float breakRatio = 1.2f;
    public float springDamping = 0.1f;

    [Header("Bend")]
    public float bendStiffness = 20f;
    public float breakAngle = 30f;
    public float bendDamping = 0.1f;

    [Header("Anchor")]
    public float anchorStiffness = 2000f;
    public float breakDistance = 0.3f;
    public float anchorDamping = 0.1f;

    [Header("Rod")]
    public float elementLength = 1f;
    public float hingeRigidJointRatio = 0f;
    public float hingeDamping = 0.01f;

    private Vector2 jointMarker;
    float breakAngleR;


    void Awake()
    {
        if (lineDrawer != null)
        {
            lines = lineDrawer.LineBuffer;
            anchorTags = lineDrawer.IsAnchored;
            jointMarker = new Vector2(0f, elementLength /2f);
            breakAngleR = breakAngle * Mathf.Deg2Rad;
            renderSpring = (lineDrawer.lineMaterial, lineDrawer.lineWidth);
        }
    }

    public void ConvertLinesToRods(

        //from parents
        List<Rod> rods,
        List<Particle> particles,
        List<Spring> springs,
        List<Bend> bends,
        List<Anchor> anchors,
        int renderIndex
        )
    {
        for (int i = 0; i < lines.Count; i++)
        {
            Vector2 start = lines[i][0];
            Vector2 end = lines[i][1];

            Particle startP = new Particle(start, mass / 2f);
            Particle endP = new Particle(end, mass / 2f);

            bool anchorStart = anchorTags[i][0];
            bool anchorEnd = anchorTags[i][1];

            //---------------------------------- start --------------------------------------

            int startIndex = particles.IndexOf(startP);
            if (startIndex == -1)
            {
                startIndex = particles.Count;
                particles.Add(startP);

                //additional particle to construct joint for torque behaviour
                Vector2 startMark = start + jointMarker;
                Particle startPMark = new Particle(startMark, mass / 2f, true);
                particles.Add(startPMark);

                if (anchorStart)
                {
                    anchors.Add(new Anchor(startIndex, start, anchorStiffness,anchorDamping, breakDistance));
                    anchors.Add(new Anchor(startIndex + 1, startMark, anchorStiffness, anchorDamping, breakDistance));
                }
            }
            else 
            {
                Particle p = particles[startIndex];
                p.Mass += mass / 2f;
                particles[startIndex] = p;

                p = particles[startIndex + 1];
                p.Mass += mass / 2f;
                particles[startIndex + 1] = p;
            }

            //------------------------------------- end ------------------------------------

            int endIndex = particles.IndexOf(endP);
            if (endIndex == -1)
            {
                endIndex = particles.Count;
                particles.Add(endP);

                //additional particle to construct joint for torque behaviour
                Vector2 endMark = end + jointMarker;
                Particle endPMark = new Particle(endMark, mass / 2f, true);
                particles.Add(endPMark);

                if (anchorEnd)
                {
                    anchors.Add(new Anchor(endIndex, end, anchorStiffness, anchorDamping, breakDistance));
                    anchors.Add(new Anchor(endIndex + 1, endMark, anchorStiffness, anchorDamping, breakDistance));
                }
            }
            else 
            {
                Particle p = particles[endIndex];
                p.Mass += mass / 2f;
                particles[endIndex] = p;

                p = particles[endIndex + 1];
                p.Mass += mass / 2f;
                particles[endIndex + 1] = p;
            }



            //----------------------------------- interpolate -------------------------------

            float length = Vector2.Distance(start, end);
            int segmentCount = Mathf.CeilToInt(length / elementLength);
            float segmentLength = length / segmentCount;

            int[] particleIndices = new int[segmentCount + 3];

            particleIndices[0] = startIndex + 1;
            particleIndices[1] = startIndex;
            particleIndices[segmentCount + 1] = endIndex;
            particleIndices[segmentCount + 2] = endIndex + 1;

            for (int j = 1; j < segmentCount; j++)
            {
                particleIndices[j + 1] = particles.Count;

                float t = (float)j / segmentCount; // 0 �� 1
                particles.Add(new Particle(Vector2.Lerp(start, end, t), mass)); 
            }

            //---------------------------------- spring & bend -------------------------------

            int[] springIndices = new int[segmentCount + 2];
            int[] bendIndices = new int[segmentCount + 1];

            for (int j = 0; j < segmentCount + 1; j++)
            {
                int A = particleIndices[j];
                int B = particleIndices[j + 1];
                int C = particleIndices[j + 2];

                float restLength;

                //Allow frictional Hinge at end points
                float restAngle;
                float hingeBendStiffness; 
                float hingeBreakAngle;
                float bendDamp;

                if (j == 0)
                {
                    restLength = elementLength / 2f;

                    //-------------------------------------

                    Vector2 pA = particles[A].Position;
                    Vector2 pB = particles[B].Position;
                    Vector2 pC = particles[C].Position;

                    Vector2 AB = pB - pA;
                    Vector2 BC = pC - pB;

                    float cross = AB.x * BC.y - AB.y * BC.x;
                    float dot = Vector2.Dot(AB, BC);

                    restAngle = Mathf.Atan2(cross, dot);

                    hingeBendStiffness = bendStiffness * hingeRigidJointRatio;
                    hingeBreakAngle = Mathf.Lerp(Mathf.PI * 2, breakAngleR, hingeRigidJointRatio);
                    bendDamp = Mathf.Lerp(hingeDamping, bendDamping, hingeRigidJointRatio);

                }
                else if (j == segmentCount)
                {
                    restLength = segmentLength;

                    //-------------------------------------

                    Vector2 pA = particles[A].Position;
                    Vector2 pB = particles[B].Position;
                    Vector2 pC = particles[C].Position;

                    Vector2 AB = pB - pA;
                    Vector2 BC = pC - pB;

                    float cross = AB.x * BC.y - AB.y * BC.x;
                    float dot = Vector2.Dot(AB, BC);

                    restAngle = Mathf.Atan2(cross, dot);

                    hingeBendStiffness = bendStiffness * hingeRigidJointRatio;
                    hingeBreakAngle = Mathf.Lerp(Mathf.PI * 2, breakAngleR, hingeRigidJointRatio);
                    bendDamp = Mathf.Lerp(hingeDamping, bendDamping, hingeRigidJointRatio);
                }
                else
                {
                    restLength = segmentLength;
                    restAngle = 0;
                    hingeBendStiffness = bendStiffness;
                    hingeBreakAngle = breakAngleR;
                    bendDamp = bendDamping;
                }

                //spring
                Spring s = new Spring(A, B, restLength, springStiffness, renderIndex, breakRatio * restLength, j != 0);
                springs.Add(s);
                springIndices[j] = springs.Count - 1;

                //bend
                Bend b = new Bend(A, B, C, restAngle, hingeBendStiffness, bendDamp, hingeBreakAngle);
                bends.Add(b);
                bendIndices[j] = bends.Count - 1;
                
                if (j == segmentCount)
                {
                    s = new Spring(B, C, elementLength / 2f, springStiffness, renderIndex, breakRatio * elementLength / 2f, false);
                    springs.Add(s);
                    springIndices[j + 1] = springs.Count - 1;
                }
            }

            rods.Add(new Rod(mass, springDamping, particleIndices, springIndices, bendIndices));
        }

    }
}
