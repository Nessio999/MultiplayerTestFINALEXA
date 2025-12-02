using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class Handgun : Wepon
{

    //LayerMask
    //Range
    //Un RPC es un protocolo para mandar allamar uin metodo en diferentes clientes
    //RpcSoces es quien leo manda a llmar 
    //RpcTargetes es quien lo ejecuta
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public override void RpcRaycastShoot(RpcInfo info = default)
    {
      if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward,out RaycastHit hit, range,layerMask))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.TryGetComponent(out Health health))
            {
                health.Rpc_TakeDamage((int)damage, info.Source);
            }
            else
            {
                //Hacer aparecer un agujero de bala
            }
        }
    }

    public override void RigidBodyShoot()
    {
        RpcPhysicShoot(shootPoint.position, shootPoint.rotation);
    }
   
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcPhysicShoot(Vector3 pos, Quaternion rot, RpcInfo info = default)
    {
        NetworkObject bulletInstance = Runner.Spawn(bullet, pos, rot, info.Source);
        bulletInstance.GetComponent<Projectile>().SetProjectile(info.Source, damage);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.indianRed;
        Gizmos.DrawRay(playerCam.transform.position, playerCam.transform.forward * range);
    }
}
