namespace GunJump.Gameplay
{
    // Lo que reacciona al impacto de una bala: enemigos y blancos del multiplicador.
    public interface IBulletTarget
    {
        void TakeHit(in HitInfo hit);
    }
}
