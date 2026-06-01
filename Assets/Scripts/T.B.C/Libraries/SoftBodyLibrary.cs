using UnityEngine;
using System.Collections.Generic;
using System;


namespace SoftBodyLibrary
{
    // Particle, Spring, Bend, Anchor
    public struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 ForceBuffer;
        public float Mass;
        public bool JointMark;

        public Particle(Vector2 pos, float mass, bool jointMark = false)
        {
            Position = pos;
            Velocity = Vector2.zero;
            JointMark = jointMark;
            Mass = mass;
            ForceBuffer = Vector2.zero;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Particle other))
                return false;

            // If this particle is NoGravity, skip IndexOf() method
            if (this.JointMark)
                return false;

            // Compare positions only
            return this.Position.Equals(other.Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public static bool operator ==(Particle a, Particle b)
        {
            return a.Position == b.Position;
        }

        public static bool operator !=(Particle a, Particle b)
        {
            return a.Position != b.Position;
        }
    }

    public struct Spring
    {
        public int IndexA;
        public int IndexB;
        public float RestLength;
        public float Stiffness; // -1 = broken
        public float MaxLengthDiff; // break threshold
        public bool JointMark; //for renderer
        public int RenderIndex;

        public Spring(int a, int b, float restLength, float stiffness, int renderIndex, float maxLength = float.PositiveInfinity, bool jointMark = true)
        {
            IndexA = a;
            IndexB = b;
            RestLength = restLength;
            Stiffness = stiffness;
            MaxLengthDiff = maxLength - restLength;
            JointMark = jointMark;
            RenderIndex = renderIndex;
        }
    }

    public struct Bend
    {
        public int IndexA;
        public int IndexB;
        public int IndexC;
        public float RestAngle;
        public float Stiffness;     // -1 = broken
        public float Damping;

        public float MaxAngleDiff;  // break threshold (radians)

        public Bend(int a, int b, int c, float restAngle, float stiffness,float bendDamp, float maxAngleDiff = float.PositiveInfinity)
        {
            IndexA = a;
            IndexB = b;
            IndexC = c;
            RestAngle = restAngle;
            Stiffness = stiffness;
            Damping = bendDamp;
            MaxAngleDiff = maxAngleDiff;
        }
    }

    public struct Anchor
    {
        public int ParticleIndex;
        public Vector2 WorldPosition;
        public float Stiffness;   // -1 = broken
        public float MaxDistance; // break threshold
        public float Damping;

        public Anchor(int index, Vector2 worldPos, float stiffness, float damping, float maxDistance = float.PositiveInfinity)
        {
            ParticleIndex = index;
            WorldPosition = worldPos;
            Stiffness = stiffness;
            MaxDistance = maxDistance;
            Damping = damping;
        }
    }

    [Serializable]
    public class Rod
    {
        public float SegmentMass;
        public float SpringDamping;

        public int[] ParticleIndices; //null means destroyed
        public int[] SpringIndices;
        public int[] BendIndices;

        public Rod(float segmentMass,float springDamping, int[] particles, int[] springs, int[] bends)
        {
            SegmentMass = segmentMass;
            SpringDamping = springDamping;

            ParticleIndices = particles;
            SpringIndices = springs;
            BendIndices = bends;
        }

        public Rod[] SplitRodAtParticle(
            int splitIndex,
            Particle[] particles,
            ref int particleLength,
            Spring[] springs,
            Bend[] bends
        )
        {
            if (splitIndex > 0 && splitIndex < ParticleIndices.Length - 1 && particleLength < particles.Length)
            {
                int p, p2;
                
                //---------------------------------------------  modify particle  -----------------------------------------------

                //1 Duplicate particle p --> p2
                p = ParticleIndices[splitIndex];
                Particle newParticle = particles[p];

                //split mass
                ref Particle oldParticle = ref particles[p];
                newParticle.Mass = SegmentMass / 2f;
                if (oldParticle.Mass >= SegmentMass)
                {
                    oldParticle.Mass -= newParticle.Mass;
                }

                //add particle
                p2 = particleLength;


                //-----------------------------------------   modify particle index  --------------------------------------------

                //2 Split ParticleIndices
                int[] leftParticles = ParticleIndices[..(splitIndex + 1)];
                int[] rightParticles = ParticleIndices[splitIndex..];

                //3 Split Springs
                int[] leftSprings = SpringIndices[..splitIndex];
                int[] rightSprings = SpringIndices[splitIndex..];

                //4 Split Bends
                int[] leftBends; //left
                if (leftSprings.Length > 1)
                    leftBends = BendIndices[..(splitIndex - 1)];
                else leftBends = new int[0];
                
                int[] rightBends; //right
                if (rightSprings.Length > 1)
                    rightBends = BendIndices[splitIndex..];
                else rightBends = new int[0];

                ref Bend bendToDeleteRef = ref bends[BendIndices[splitIndex - 1]];
                bendToDeleteRef.Stiffness = -1; // mark broken

                //5 Replace particle
                if (rightBends.Length > 0)
                {
                    rightParticles[0] = p2;

                    ref Spring firstRightSpringRef = ref springs[rightSprings[0]];
                    firstRightSpringRef.IndexA = p2;

                    ref Bend firstRightBendRef = ref bends[rightBends[0]];
                    firstRightBendRef.IndexA = p2;
                }
                else if (leftBends.Length > 0)
                {
                    leftParticles[leftParticles.Length - 1] = p2;

                    ref Spring lastLeftSpringRef = ref springs[leftSprings[leftSprings.Length - 1]];
                    lastLeftSpringRef.IndexB = p2;

                    ref Bend lastLeftBendRef = ref bends[leftBends[leftBends.Length - 1]];
                    lastLeftBendRef.IndexC = p2;
                }

                else if (!springs[rightSprings[0]].JointMark)
                {
                    rightParticles[0] = p2;
                    ref Spring firstRightSpringRef = ref springs[rightSprings[0]];
                    firstRightSpringRef.IndexA = p2;
                }
                else if (!springs[leftSprings[0]].JointMark)
                {
                    leftParticles[leftParticles.Length - 1] = p2;
                    ref Spring lastLeftSpringRef = ref springs[leftSprings[leftSprings.Length - 1]];
                    lastLeftSpringRef.IndexB = p2;
                }
                else return null;

                particles[particleLength] = newParticle;
                particleLength++;

                Rod left = new Rod(SegmentMass, SpringDamping, leftParticles, leftSprings, leftBends);
                Rod right = new Rod(SegmentMass, SpringDamping, rightParticles, rightSprings, rightBends);
                return new Rod[] { left, right };

            }
            else
            {
                return null;
            }

        }

    }

    public class SoftBodySystem
    {
        private Rod[] rods; // particle count
        private int rodLength; 

        public Particle[] particles; //2 times of particle count
        private int particleLength; 

        public Spring[] springs;
        private Bend[] bends;
        private Anchor[] anchors;
        private Vector2 gravity;
        private float deltaTime;
        private float drag;
        private float friction;
        private float restitution;
        private float maxSpeed;
        private LayerMask staticColliderMask;
        private LayerMask rigidColliderMask;

        public SoftBodySystem(Rod[] rods,
            Particle[] particles,
            Spring[] springs,
            Bend[] bends,
            Anchor[] anchors,
            Vector2 gravity,
            float deltaTime,
            float drag,
            float maxSpeed,
            float friction,
            float restitution
        )
        {
            this.rodLength = rods.Length;
            this.particleLength = particles.Length;

            Array.Resize(ref rods, rods.Length + particles.Length);
            Array.Resize(ref particles, particles.Length * 2);

            this.rods = rods;
            this.particles = particles;


            this.springs = springs;
            this.bends = bends;
            this.anchors = anchors;
            this.gravity = gravity;
            this.deltaTime = deltaTime;
            this.drag = drag;
            this.maxSpeed = maxSpeed;
            this.friction = friction;
            this.restitution = restitution;
            this.staticColliderMask = LayerMask.GetMask("StaticCollider");
            this.rigidColliderMask = LayerMask.GetMask("RigidCollider");
        }

        public void Step()
        {
            //compute rod force & mark break
            int rodCount = rodLength;

            for (int i = 0; i < rodCount; i++)
            {

                Rod rod = rods[i];
                int splitIndex = -1;

                int breakIndex = -1;
                int breakTag = -1; //-1: none, 0:spring, 1:bend
                float maxBreak = 0;

                //springs in a rod

                int[] springIndices = rod.SpringIndices;

                for (int j = 0; j < springIndices.Length; j++)
                {
                    ref Spring s = ref springs[springIndices[j]];

                    if (s.Stiffness < 0) continue; // broken

                    ref Particle pA = ref particles[s.IndexA];
                    ref Particle pB = ref particles[s.IndexB];

                    Vector2 delta = pB.Position - pA.Position;
                    float dist = delta.magnitude;
                    if (dist < 0.0001)
                    {
                        s.Stiffness = -1; continue;
                    }

                    Vector2 AB = delta / dist;
                    float deltaX = dist - s.RestLength;
                    Vector2 force = s.Stiffness * deltaX * AB;

                    

                    //FORCE
                    pA.ForceBuffer += force;
                    pB.ForceBuffer -= force;


                    // Break the longest spring stretched beyond max length
                    float breakRatio = deltaX / s.MaxLengthDiff;
                    if (breakRatio > 1 && breakRatio > maxBreak)
                    {
                        maxBreak = breakRatio;
                        breakIndex = j;
                        breakTag = 0;
                    }
                    else if(breakRatio<=1)
                    {
                        //Damping
                        Vector2 AB_DAMP = Vector2.Dot(pA.Velocity - pB.Velocity, AB) * AB * rod.SpringDamping;
                        pA.Velocity -= AB_DAMP;
                        pB.Velocity += AB_DAMP;
                    }
                }

                //bends in a rod

                int[] bendIndices = rod.BendIndices;

                for (int j = 0; j < bendIndices.Length; j++)
                {
                    ref Bend b = ref bends[bendIndices[j]];

                    if (b.Stiffness < 0) continue; // broken

                    ref Particle pA = ref particles[b.IndexA];
                    ref Particle pB = ref particles[b.IndexB];
                    ref Particle pC = ref particles[b.IndexC];

                    Vector2 AB = pB.Position - pA.Position;
                    Vector2 BC = pC.Position - pB.Position;

                    float cross = AB.x * BC.y - AB.y * BC.x;
                    float dot = Vector2.Dot(AB, BC);
                    float currentAngle = Mathf.Atan2(cross, dot);

                    float angleDiff = currentAngle - b.RestAngle;
                    if (angleDiff > Mathf.PI)
                        angleDiff -= 2 * Mathf.PI;
                    else if (angleDiff < -Mathf.PI)
                        angleDiff += 2 * Mathf.PI;

                    float magAB = Mathf.Sqrt(AB.x * AB.x + AB.y * AB.y);
                    Vector2 perpAB = new Vector2(-AB.y / magAB, AB.x / magAB);

                    float magBC = Mathf.Sqrt(BC.x * BC.x + BC.y * BC.y);
                    Vector2 perpBC = new Vector2(-BC.y / magBC, BC.x / magBC);

                    float breakRatio = Mathf.Abs(angleDiff) / b.MaxAngleDiff;

                    Vector2 fAB = perpAB * angleDiff * b.Stiffness;
                    Vector2 fBC = perpBC * angleDiff * b.Stiffness;
                    
                    //FORCE
                    pA.ForceBuffer -= fAB;
                    pB.ForceBuffer += (fAB + fBC);
                    pC.ForceBuffer -= fBC;

                    // Break the biggest angle exceeds threshold

                    if (breakRatio > 1 && breakRatio > maxBreak)
                    {
                        maxBreak = breakRatio;
                        breakIndex = j;
                        breakTag = 1;
                    }
                    else if (breakRatio <= 1)
                    {
                        //Damping
                        float sAB_perpAB = Vector2.Dot(pA.Velocity - pB.Velocity, perpAB);
                        float sCB_perpBC = Vector2.Dot(pC.Velocity - pB.Velocity, perpBC);

                        float omegaAC = sAB_perpAB / magAB + sCB_perpBC / magBC;
                        float lengthRatioAB = magAB / (magAB + magBC);
                        float lengthRatioBC = 1 - lengthRatioAB;

                        if (Mathf.Abs(omegaAC) > 0)
                        {
                            Vector2 AB_DAMP = b.Damping * sAB_perpAB * b.Damping * perpAB * breakRatio * lengthRatioAB;
                            Vector2 CB_DAMP = b.Damping * sCB_perpBC * b.Damping * perpBC * breakRatio * lengthRatioBC;

                            pA.Velocity -= AB_DAMP;
                            pB.Velocity += AB_DAMP + CB_DAMP;
                            pC.Velocity -= CB_DAMP;
                        }
                    }
                }

                if (breakIndex != -1 && rodLength < rods.Length)
                {
                    // DEBUG: see what caused the break
                    string cause = breakTag == 0 ? "spring" : "bend" ;
                    Debug.Log($"Breaking rod {i} at index {breakIndex} due to {cause}");

                    splitIndex = breakIndex + breakTag;
                    splitIndex = splitIndex < 1 ? 1 : splitIndex;

                    Rod[] tempRods = rods[i].SplitRodAtParticle(splitIndex, particles, ref particleLength, springs, bends);

                    if (tempRods != null)
                    {
                        rods[i] = tempRods[0];
                        rods[rodLength] = tempRods[1];
                        rodLength++;
                    }
                }
            }


            //anchor
            for (int i = 0; i < anchors.Length; i++)
            {
                ref Anchor a = ref anchors[i];
                if (a.Stiffness < 0) continue; // broken

                ref Particle p = ref particles[a.ParticleIndex];
                Vector2 delta = p.Position - a.WorldPosition;
                float dist = delta.magnitude;

                if (dist < 0.001) continue;
                else if (dist > a.MaxDistance) { a.Stiffness = -1; continue; }
               
                //damping
                Vector2 direction = delta / dist;

                Vector2 p_DAMP = Vector2.Dot(p.Velocity, direction) * direction * a.Damping;
                p.Velocity -= p_DAMP;

                Vector2 force = -delta * a.Stiffness;
                p.ForceBuffer += force;
            }

            //gravity & drag & move
            float r = 0.1f;
            for (int i = 0; i < particleLength; i++)
            {

                particles[i].Velocity += particles[i].ForceBuffer * deltaTime / particles[i].Mass;
                particles[i].ForceBuffer = Vector2.zero;

                if (!particles[i].JointMark)
                {
                    particles[i].Velocity += gravity * deltaTime;
                }

                //predict distance
                Vector2 position = particles[i].Position;
                Vector2 move = particles[i].Velocity * deltaTime;
                Vector2 predictedPosition = position + move;
                float time = deltaTime;

                float dist = move.magnitude;
                if (dist < 0.00001) { continue; }
                
                //collision
                Vector2 dir = move / dist;
                RaycastHit2D hit = Physics2D.CircleCast(particles[i].Position, r, dir, dist, staticColliderMask);
                if (hit.collider && !particles[i].JointMark)
                {
                    float vn = Vector2.Dot(particles[i].Velocity, hit.normal);

                    if (vn < 0)
                    {
                        position = hit.distance < 0.001f ? position : (hit.point + hit.normal * r);
                        time = hit.distance < 0.001f ? deltaTime : (1f - hit.distance / dist) * deltaTime;

                        Vector2 vN = vn * hit.normal;
                        Vector2 vT = particles[i].Velocity - vN;

                        particles[i].Velocity = -restitution * vN + friction * vT;
                    }
                }

                particles[i].Velocity *= drag;

                float speed = particles[i].Velocity.magnitude;
                if (speed > maxSpeed)
                {
                    particles[i].Velocity /= speed;
                    particles[i].Velocity *= maxSpeed;
                }

                particles[i].Position = position + particles[i].Velocity * time;
            }
        }
    }


}

    
