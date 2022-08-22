using System;
using UnityEngine;

namespace UniversalMachine
{
    public class Particle : MonoBehaviour
    {
        public static float Nearest = 1.4013e-25f;
        public static float EnergeticResistance = 10f;
        public static float DimensionalDelta = 10f;
        public static float KineticEasing = 100f;
        public static float ContactWindow = 0.01f;

        public class Folding
        {
            public Vector3 Energy;
            public Vector3 Position;
            public Vector3 Force;
            public Vector3 Torque;
            public float Dimensionality;
        }

        public enum StateFocus
        {
            Energy = 1,
            Position = -1
        }

        public enum EffectorFolding
        {
            Energy,
            Position,
            Force,
            Torque,
            NonSystematic
        }


        public Vector4 Energy = new Vector4(0,0,0, 0);
        public Vector4 Position = new Vector4(0,0,0, 0);

        Vector4 Force = new Vector4(0,0,0, 0);
        Vector4 Torque = new Vector4(0,0,0, 0);

        //Energetic, Solid, Spacial, Mechanical
        public Vector4 Descriptor = new Vector4();

        public float Age { get; private set; }

        public int SlamEvents = 0;

        public double SlamConsideration;

        public float ContactDepth;

        public delegate void OnDestroyAction();
        public OnDestroyAction onDestroy;

        public Func<double, double> PrimaryReduction
        {
            get
            {
                return new Func<double, double>((regionalArea) =>
                 {
                     double syphon = 1 / ((Vector3)Energy).magnitude * (ContactDepth / regionalArea);

                     Energy = new Vector4(
                         Energy.x - Energy.x * (float)syphon,
                         Energy.y - Energy.y * (float)syphon,
                         Energy.z - Energy.z * (float)syphon,
                         Energy.w - Energy.w * (float)syphon < 0 ? 0 : Energy.w - Energy.w * (float)syphon);

                     return syphon * ContactWindow;
                 });
            }
        }

        public Func<double, double> SecondaryReduction
        {
            get
            {
                return new Func<double, double>((regionalArea) =>
                {
                    double syphon = 1 / Age * ContactDepth * regionalArea;
                    
                    Energy = new Vector4(
                        Energy.x - Energy.x * (float)syphon,
                        Energy.y - Energy.y * (float)syphon,
                        Energy.z - Energy.z * (float)syphon,
                        Energy.w - Energy.w * (float)syphon < 0 ? 0 : Energy.w - Energy.w * (float)syphon);
                    
                    return syphon * ContactWindow;
                });
            }
        }

        public Func<double, double> TertiaryReduction
        {
            get
            {
                return new Func<double, double>((regionalArea) =>
                {
                    double fAvg = (Force.x + Force.y + Force.z) / 3;
                    double syphon = 1 / fAvg * ContactDepth / regionalArea;

                    Energy = new Vector4(
                        Energy.x - Energy.x * (float)syphon,
                        Energy.y - Energy.y * (float)syphon,
                        Energy.z - Energy.z * (float)syphon,
                        Energy.w - Energy.w * (float)syphon < 0 ? 0 : Energy.w - Energy.w * (float)syphon);

                    return syphon * ContactWindow;
                });
            }
        }

        public Func<Vector3, Vector3, Vector3, Vector3> Project;

        System.Random r = new System.Random();

        public float GetDiscernmentRatio(float discernment, float deltaTime)
        {
            if (float.IsNaN(discernment) || discernment == 0)
            {
                //Debug.Log("Lowered Dimensional Number: " + 0);
                return deltaTime;
            }

            return discernment * deltaTime;
        }

        public void AddForce(Vector3 f, Vector3 point, float deltaTime)
        {
            Vector3 contactForce;
            float energyMagnitude = PointEnergy(deltaTime).magnitude;

            if (energyMagnitude == 0)
            {
                contactForce = f / KineticEasing * ContactDepth;
            }
            else
            {
                contactForce = f / (KineticEasing * (energyMagnitude / EnergeticResistance)) * ContactDepth;
            }
 

            float totalDiscernment = Position.w * Energy.w * Mathf.Pow(Torque.w, 2) * Mathf.Pow(Force.w, 3);
            float discernmentRatio = GetDiscernmentRatio(totalDiscernment, deltaTime);


            Vector3 pf = PointForce(deltaTime);
            Force = new Vector4(
                pf.x + contactForce.x,
                pf.y + contactForce.y,
                pf.z + contactForce.z,
                1f);

            Vector3 pt = PointTorque(deltaTime);
            Torque = new Vector4(
                pt.x + point.x,
                pt.y + point.y,
                pt.z + point.z,
                1f);
        }

        

        public Vector4 Cross(Vector4 vector1, Vector4 vector2)
        {
            return new Vector4(
                vector1.y * vector2.z - vector1.z * vector2.y,
                vector1.z * vector2.x - vector1.x * vector2.z,
                vector1.x * vector2.y - vector1.y * vector2.x,
                vector1.w * vector2.w);
        }

        public Vector3 Mul(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(
                vector1.x * vector2.x,
                vector1.y * vector2.y,
                vector1.z * vector2.z
                );
        }

        public float GetLoweredDimensionalNumber(Vector4 detail, float deltaTime)
        {
            float number = DimensionalDelta * detail.w * deltaTime;

            if (float.IsNaN(number))
            {
                //Debug.Log("Lowered Dimensional Number: " + 0);
                return 0;
            }
            else if (float.IsInfinity(number))
            {
                //Debug.Log("Lowered Dimensional Number: " + 1);
                return 1;
            }
            
            return number;
        }

        public float GetDiscernmentNumber(float loweredDimensionalNumber)
        {
            //return Mathf.Sin(Age) * loweredDimensionalNumber;
            return loweredDimensionalNumber;
        }
        
        public Vector3 PointPosition(float deltaTime)
        {
            float t = GetLoweredDimensionalNumber(Position, deltaTime);
            Vector3 pPosition =  new Vector3(
                Position.x * GetDiscernmentNumber(t),
                Position.y * GetDiscernmentNumber(t),
                Position.z * GetDiscernmentNumber(t)
                );
            return pPosition;

            //return new Vector3(
            //    Position.x * Position.w,
            //    Position.y * Position.w,
            //    Position.z * Position.w);
        }

        public Vector3 PointEnergy(float deltaTime)
        {
            float t = GetLoweredDimensionalNumber(Energy, deltaTime);
            Vector3 pEnergy = new Vector3(
                Energy.x * GetDiscernmentNumber(t),
                Energy.y * GetDiscernmentNumber(t),
                Energy.z * GetDiscernmentNumber(t)
                );
            return pEnergy;
        }

        public Vector3 PointForce(float deltaTime)
        {
            float t = GetLoweredDimensionalNumber(Force, deltaTime);
            Vector3 pForce = new Vector3(
                Force.x * GetDiscernmentNumber(t),
                Force.y * GetDiscernmentNumber(t),
                Force.z * GetDiscernmentNumber(t)
                );
            return pForce;
        }

        public Vector3 PointTorque(float deltaTime)
        {
            float t = GetLoweredDimensionalNumber(Torque, deltaTime);
            Vector3 pTorque = new Vector3(
                Torque.x * GetDiscernmentNumber(t),
                Torque.y * GetDiscernmentNumber(t),
                Torque.z * GetDiscernmentNumber(t)
                );
            return pTorque;
        }

        public Vector3 TotalEnergy()
        {
            return new Vector3(Energy.x * Energy.w, Energy.y * Energy.w, Energy.z * Energy.w);
        }

        public Vector3 TotalForce()
        {
            return new Vector3(Force.x * Force.w, Force.y * Force.w, Force.z * Force.w);
        }

        public Vector3 TotalTorque()
        {
            return new Vector3(Torque.x * Torque.w, Torque.y * Torque.w, Torque.z * Torque.w);
        }

        public void Simulate(float deltaTime)
        {
            //Debug.Log("A: " + Position + " | " + Energy + " | " + Force + " | " + Torque);
            //float forceOffset = 1 / Force.w * deltaTime;
            //float torqueOffset = 1 / Torque.w  * deltaTime;

            float totalDiscernment = Force.w * Torque.w;
            float discernmentRatio = GetDiscernmentRatio(totalDiscernment, deltaTime);

            Vector3 pPos = PointPosition(deltaTime);
            Vector3 pEnergy = PointEnergy(deltaTime);
            Vector3 pTorque = PointTorque(deltaTime);
            Vector3 pForce = PointForce(deltaTime);

            Vector3 pos = new Vector3(
                pPos.x + ((pForce.x * pTorque.x)), // / pEnergy.x),
                pPos.y + ((pForce.y * pTorque.y)), // / pEnergy.y),
                pPos.z + ((pForce.z * pTorque.z)) // / pEnergy.z)
                );

            Vector3 projectedPos = Project(pPos, pos, pEnergy);

            float discrnUp = Position.w / ((Vector3)Position).magnitude * projectedPos.magnitude;

            //projectedPos = pos;

            float currentPosMag = pPos.magnitude - projectedPos.magnitude;
            
            float positionUsage = Position.w - (1 / pPos.magnitude * currentPosMag);

            Position = new Vector4(projectedPos.x,
                projectedPos.y,
                projectedPos.z,
                discrnUp);

            IndiscernPositioning(positionUsage, deltaTime);
            IndiscernForceAndTorque(discernmentRatio, deltaTime);
        }

        public void IndiscernPositioning(float ratio, float deltaTime)
        {
            float x = r.Next(-1, 1) * (float)r.NextDouble() * Position.magnitude;
            float y = r.Next(-1, 1) * (float)r.NextDouble() * Position.magnitude;
            float z = r.Next(-1, 1) * (float)r.NextDouble() * Position.magnitude;

            Vector3 discerned = new Vector3(Position.x * ratio, Position.y * ratio, Position.z * ratio);

            Vector3 position = new Vector3(Position.x + x, Position.y + y, Position.z + z) - discerned;

            float discernment = ratio * deltaTime;
            float positionDimensionNumber = Position.w - discernment > 0 ? Position.w - discernment : 0;

            Position = new Vector4(position.x, position.y, position.z, positionDimensionNumber);
        }

        public void IndiscernForceAndTorque(float ratio, float deltaTime)
        {
            float x = r.Next(-1, 1) * (float)r.NextDouble() * Force.magnitude;
            float y = r.Next(-1, 1) * (float)r.NextDouble() * Force.magnitude;
            float z = r.Next(-1, 1) * (float)r.NextDouble() * Force.magnitude;

            Vector3 force = new Vector3(Force.x + x - (Force.x * ratio),
                Force.y + y - (Force.y * ratio),
                Force.z + z - (Force.z * ratio));

            x = r.Next(-1, 1) * (float)r.NextDouble() * Torque.magnitude;
            y = r.Next(-1, 1) * (float)r.NextDouble() * Torque.magnitude;
            z = r.Next(-1, 1) * (float)r.NextDouble() * Torque.magnitude;

            Vector3 torque = new Vector3(Torque.x + x - (Torque.x * ratio),
                Torque.y + y - (Torque.y * ratio),
                Torque.z + z - (Torque.z * ratio));

            float discernment = ratio * deltaTime;
            float forceDimensionNumber = Force.w - discernment > 0 ? Force.w - discernment : 0;
            float torqueDimensionNumber = Torque.w - discernment > 0 ? Torque.w - discernment : 0;

            Force = new Vector4(force.x, force.y, force.z, forceDimensionNumber);
            Torque = new Vector4(torque.x, torque.y, torque.z, torqueDimensionNumber);
        }
        
        public void Fold(EffectorFolding target, Vector3 foldingDelta)
        {
            switch (target)
            {
                case EffectorFolding.Energy:
                    Energy = EnergyEffector(foldingDelta);
                    break;
                case EffectorFolding.Position:
                    Position = PositionEffector();
                    break;
                case EffectorFolding.Torque:
                    Torque = AngularEffector();
                    break;
                case EffectorFolding.Force:
                    Force = ForceEffector();
                    break;
                case EffectorFolding.NonSystematic:
                    Expostulate(Time.deltaTime);
                    break;
            }
        }

        public Folding Postulate()
        {
            float Dimensionality = Energy.w + Position.w + Torque.w + Force.w;

            Vector3 ener = new Vector3(Energy.x * Energy.w, Energy.y * Energy.w, Energy.z * Energy.w);
            Energy = new Vector4();

            Vector3 pos = new Vector3(Position.x * Position.w, Position.y * Position.w, Position.z * Position.w);
            Position = new Vector4();

            Vector3 tor = new Vector3(Torque.x * Torque.w, Torque.y * Torque.w, Torque.z * Torque.w);
            Torque = new Vector4();

            Vector3 four = new Vector3(Force.x * Force.w, Force.y * Force.w, Force.z * Force.w);
            Force = new Vector4();

            Folding information = new Folding();
            information.Energy = ener;
            information.Position = pos;
            information.Torque = tor;
            information.Force = four;
            
            return information;
        }

        public Vector4 EnergyEffector(Vector3 effectorDelta)
        {
            float dimensionality = (Position.w + Torque.w + Force.w) / 3 * effectorDelta.magnitude;

            Vector3 pos = new Vector3(
                Position.x * Position.w * effectorDelta.x,
                Position.y * Position.w * effectorDelta.y,
                Position.z * Position.w * effectorDelta.z
                );

            float posDisc = Position.w / ((Vector3)Position).magnitude * pos.magnitude;
            Position = Position - new Vector4(pos.x, pos.y, pos.z, posDisc);

            Vector3 tor = new Vector3(
                Torque.x * Torque.w * effectorDelta.x,
                Torque.y * Torque.w * effectorDelta.y,
                Torque.z * Torque.w * effectorDelta.z
                );

            float torDisc = Torque.w / ((Vector3)Torque).magnitude * tor.magnitude;
            Torque = Torque - new Vector4(tor.x, tor.y, tor.z, torDisc);

            Vector3 forc = new Vector3(
                Force.x * Force.w * effectorDelta.x,
                Force.y * Force.w * effectorDelta.y,
                Force.z * Force.w * effectorDelta.z
                );

            float forDisc = Force.w / ((Vector3)Force).magnitude * forc.magnitude;
            Force = Force - new Vector4(forc.x, forc.y, forc.z, forDisc);

            Vector4 specifics =  pos + (Mul(tor, forc));
            specifics.w = dimensionality;
            return specifics;
        }

        public Vector4 PositionEffector()
        {
            float dimensionality = Energy.w + Torque.w + Force.w;

            Vector3 ener = new Vector3(Energy.x * Energy.w, Energy.y * Energy.w, Energy.z * Energy.w);
            Energy = new Vector4();

            Vector3 tor = new Vector3(Torque.x * Torque.w, Torque.y * Torque.w, Torque.z * Torque.w);
            Torque = new Vector4();

            Vector3 four = new Vector3(Force.x * Force.w, Force.y * Force.w, Force.z * Force.w);
            Force = new Vector4();

            Vector4 specifics = tor + four;
            specifics = new Vector4(specifics.x * ener.x, specifics.y * ener.y, specifics.z * ener.z);
            specifics.w = dimensionality;
            return specifics;
        }

        public Vector4 AngularEffector()
        {
            float dimensionality = Energy.w + Position.w + Force.w;

            Vector3 ener = new Vector3(Energy.x * Energy.w, Energy.y * Energy.w, Energy.z * Energy.w);
            Energy = new Vector4();

            Vector3 pos = new Vector3(Position.x * Position.w, Position.y * Position.w, Position.z * Position.w);
            Position = new Vector4();

            Vector3 four = new Vector3(Force.x * Force.w, Force.y * Force.w, Force.z * Force.w);
            Force = new Vector4();

            Vector4 specifics = new Vector3(ener.x * four.x, ener.y * four.y, ener.z * four.z) + pos;
            specifics.w = dimensionality;
            return specifics;
        }

        public Vector4 ForceEffector()
        {
            float dimensionality = Energy.w + Position.w + Torque.w;

            Vector3 ener = new Vector3(Energy.x * Energy.w, Energy.y * Energy.w, Energy.z * Energy.w);
            Energy = new Vector4();

            Vector3 pos = new Vector3(Position.x * Position.w, Position.y * Position.w, Position.z * Position.w);
            Position = new Vector4();

            Vector3 tor = new Vector3(Torque.x * Torque.w, Torque.y * Torque.w, Torque.z * Torque.w);
            Torque = new Vector4();

            Vector4 specifics = new Vector3(ener.x * tor.x, ener.y * tor.y, ener.z * tor.z) + pos;
            specifics.w = dimensionality;
            return specifics;
        }

        //ReinitializationalParamaterlessFunctionalitySystem
        //A.K.A. Role-Playing File System
        //A.K.A. Regional Protection Forwarding Service
        //A.K.A. Relentless Persona Friction State
        //A.K.A. Only William Really Knows What It Does! Till Now :)
        public float Expostulate(float deltaTime)
        {
            float dimensionality = Energy.w + Position.w + Torque.w + Force.w;

            float primordials = Mathf.Pow(Energy.x, deltaTime);
            primordials += Mathf.Pow(Energy.y, deltaTime);
            primordials += Mathf.Pow(Energy.z, deltaTime);
            
            primordials += Mathf.Pow(Position.x, deltaTime);
            primordials += Mathf.Pow(Position.y, deltaTime);
            primordials += Mathf.Pow(Position.z, deltaTime);

            primordials += Mathf.Pow(Torque.x, deltaTime);
            primordials += Mathf.Pow(Torque.y, deltaTime);
            primordials += Mathf.Pow(Torque.z, deltaTime);

            primordials += Mathf.Pow(Force.x, deltaTime);
            primordials += Mathf.Pow(Force.y, deltaTime);
            primordials += Mathf.Pow(Force.z, deltaTime);

            primordials /= Mathf.Pow(deltaTime, 3);
            primordials *= Mathf.Pow(dimensionality, 5);

            primordials *= Mathf.Pow(1, deltaTime);

            return primordials;
        }

        public void Advance(float deltaTime)
        {
            float energySolve = Descriptor.x * Energy.w * deltaTime;
            Energy = new Vector4(Energy.x * energySolve, Energy.y * energySolve, Energy.z * energySolve, Energy.w - energySolve);

            float positionSolve = Descriptor.y * Position.w * deltaTime;
            Position = new Vector4(Position.x * positionSolve, Position.y * positionSolve, Position.z * positionSolve, Position.w - positionSolve);

            float forceSolve = Descriptor.z * Force.w * deltaTime;
            Force = new Vector4(Force.x * forceSolve, Force.y * forceSolve, Force.z * forceSolve, Force.w - forceSolve);

            float torqueSolve = Descriptor.w * Torque.w * deltaTime;
            Torque = new Vector4(Torque.x * torqueSolve, Torque.y * torqueSolve, Torque.z * torqueSolve, Torque.w - torqueSolve);
        }

        public void Reduce(float delta, Func<double, double, double> algorithmic, Func<Particle> stream)
        {
            
        }

        public void SetPosition(float deltaTime)
        {
            transform.localPosition = PointPosition(deltaTime);
        }

        float pause;

        // Update is called once per frame
        void FixedUpdate()
        {
            //if (pause < 1f)
            //{
            //    pause += Time.deltaTime;
            //    return;
            //}
            //pause = 0f;

            Age += Time.deltaTime;

            Simulate(Time.deltaTime);
            //Advance(Time.deltaTime);

            SetPosition(Time.deltaTime);

            //transform.localPosition = (Vector3)Move(Time.deltaTime);
        }

        void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}