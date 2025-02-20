using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TeamingEnemy : Enemy
{
    public int myNumber;
    public GameObject AnyAwareOf { get => TeamController.anyAwareOf; }

    public virtual void InitializeEnemy()
    {
        obstacleLayer = LayerMask.GetMask("Wall");
        myNumber = TeamController.team.Count;
        TeamController.team.Add(this);
        TeamController.teamTotal++;
    }

    public override int PowerLevel(float health, float maxHealth) 
    {
        float totalHealth = 0;
        float totalMaxHealth = 0;
        foreach (TeamingEnemy teamer in TeamController.team)
        {
            totalHealth += teamer.health;
            totalMaxHealth += teamer.maxHealth;
        }
        return Mathf.RoundToInt(((totalHealth + 3) / (totalMaxHealth + 3)) * powerLevel * TeamController.teamTotal);
    }

    public void SetTeamEstablished() {TeamController.teamEstablished = true;}

    public override Collider2D[] GetPossibleFocuses(Vector3 pos, float range)
    {
        HashSet<Collider2D> set = TeamController.GetTeamConcerns();
        Collider2D[] arr = new Collider2D[set.Count];
        set.CopyTo(arr);
        // Debug.Log(arr.Length);
        return arr;
    }

    public override void Die()
    {
        if (!dead)
        {
            TeamController.team.Remove(this);
            TeamController.teamTotal--;
            if (itemSpawner) { gameObject.GetComponent<ItemSpawner>().SpawnItem(); }
        }
        dead = true;
        Destroy(gameObject);
    }

    protected class TeamController
    {
        public static List<TeamingEnemy> team = new List<TeamingEnemy>();

        public static int teamTotal = 0;

        public static bool teamEstablished = false;
        /// <summary>Whether any members of pack are alert to something</summary>
        public static bool anyAware = false;
        /// <summary>Some GameObject any is aware of</summary>
        public static GameObject anyAwareOf;

        public static HashSet<Collider2D> GetTeamConcerns()
        {
            HashSet<Collider2D> concerns = new HashSet<Collider2D>();
            foreach (TeamingEnemy teamer in team)
            {
                if (teamer == null || teamer.dead) continue;
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(teamer.transform.position, teamer.range))
                {
                    concerns.Add(collider);
                }
            }
            return concerns;
        }

        /// <summary>Alerts enemies to presence of new thing for 3 seconds because it was hit</summary>
        public static IEnumerator AlertAll(GameObject hitter)
        {
            anyAware = true;
            anyAwareOf = hitter;
            yield return new WaitForSeconds(3);
            anyAware = false;
            anyAwareOf = null;
        }
    }
}
