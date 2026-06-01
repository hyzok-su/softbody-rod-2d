# Overview

This project is a Unity 2d softbody rod simulation program based on mass-spring system, additionally using joints, orthogonally decoupled damper, and topological tearing. By modifying the parameters, the framework can represent different material behaviors, ranging from rope-like motion to beam-like structures and hinge-based pendulum motion.

<img src="./docs/wholeSystemDemo.gif" width="30%"/> 

## Orthogonal Damper

In contrast to conventional mass–spring systems, which typically employ a single global damping term applied uniformly to particle velocities, the proposed method decomposes damping into multiple physically interpretable components to achieve more controlled and anisotropic energy dissipation. Specifically, damping is separated into spring-aligned damping, rotational (bending) damping, and aerodynamic drag.

Spring damping acts along the tangent direction of the beam segment, corresponding to the line-of-action velocity between connected particles, and primarily attenuates axial oscillations. Rotational damping, defined in the perpendicular direction to the beam segment, is applied to velocity components that contribute to angular motion and bending, thereby directly controlling curvature-induced oscillations. Aerodynamic drag introduces an additional global dissipation term that scales with velocity magnitude, modeling ambient resistance.

<p>
<img src="./docs/nodampingRope.gif" height="250" width="20%"/>  <img src="./docs/damping&dragRope.gif" height="250" width="20%"/> <img src="./docs/dampingRope.gif" height="250" width="20%"/>
<p/>
  
<p>
<img src="./docs/nodamping.gif" height="250" width="20%"/>    <img src="./docs/damping&drag.gif" height="250" width="20%"/>   <img src="./docs/damping.gif" height="250" width="20%"/>
<p/>
  
<p>
<img src="./docs/nodampingPendulum.gif" height="200" width="30%"/>    <img src="./docs/conventionalDampingPendulum.gif" height="200" width="30%"/>   <img src="./docs/ourdampingPendulum.gif" height="200" width="30%"/>
<p/>

## Joint

<p>
<img src="./docs/hingeJoint.gif" height="235" width="30%"/>   <img src="./docs/partialJoint.gif" height="235" width="30%"/>  <img src="./docs/rigidJoint.gif" height="235" width="30%"/>   
<p/>

## Tearing

<p>
<img src="./docs/notearing.gif" height="300" width="30%"/>   <img src="./docs/tearing.gif" height="300" width="30%"/>  
<p/>
  
## Collision

<p>
<img src="./docs/staticfriction.gif" height="235" width="20%"/>   <img src="./docs/nofriction.gif" height="235" width="20%"/>  <img src="./docs/fullrestitution.gif" height="235" width="20%"/>    <img src="./docs/norestitution.gif" height="235" width="20%"/>  
<p/>
