using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// The type of enemy: basic or unique behavior
/// </summary>
public enum EnemyType
{
    basic,
    circler,
    charger
};

/// <summary>
/// What makes an enemy attack, <br/>
/// Cornered: Will attack only if it believes it's cornered <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Aggressive: Will seek out to attack
/// </summary>
[System.Serializable]
public enum WhenAttack
{
    Cornered,
    AttackedFirst,
    InTerritory,
    Always
}

/// <summary>
/// What makes an enemy attack, <br/>
/// Cornered: Will attack only if it believes it's cornered <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Aggressive: Will seek out to attack
/// </summary>
[System.Serializable]
public enum WhenFollow
{
    IfAttacked,
    IfImHome,
    IfImStronger,
    Always
}

/// <summary>
/// What makes an enemy attack, <br/>
/// Cornered: Will attack only if it believes it's cornered <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Territorial: Will attack if you get close enough, won't wander much if at all <br/>
/// Aggressive: Will seek out to attack
/// </summary>
[System.Serializable]
public enum WhenFlee
{
    IfAttacked,
    IfImNotHome,
    IfImWeaker,
    Always
}

/// <summary>Whether to prioritize Items or Characters and by how much</summary>
[System.Serializable]
public struct BehaviorPriority
{
    /// <summary>Whether to prioritize Items or Characters or neither</summary>
    public FocusPriority focusType;
    /// <summary>How much to add to prioritize this type</summary>
    public int addend;
    /// <summary>Whether to prioritize Items or Characters and by how much</summary>
    /// <param name="addend">How much to add to this type of priority</param>
    /// <param name="focusType">Whether to prioritize Items or Characters or neither</param>
    public BehaviorPriority(int addend, FocusPriority focusType)
    {
        this.focusType = focusType;
        if (focusType == FocusPriority.none) this.addend = 0;
        else this.addend = addend;
    }
}

/// <summary>Whether an enemy should prioritize Characters, Items, or neither</summary>
[System.Serializable]
public enum FocusPriority
{
    character,
    item,
    none
}

public abstract class Enemy : Character
{
    /// <summary>List of <see cref="Relationship"/>s with anything else</summary>
    [Header("----------Behavior----------")]
    [SerializeField]
    public List<Relationship> relationships = new List<Relationship>();
    /// <summary>List of <see cref="Relationship"/>s with items</summary>
    public List<ItemRelationship> itemRelationships = new List<ItemRelationship>();
    /// <summary>How do I behave with other <see cref="Character"/>s by default</summary>
    public CharacterBehavior defaultBehavior;
    /// <summary>How do I behave with <see cref="Item"/>s by default</summary>
    public CharacterBehavior itemDefaultBehavior;
    /// <summary>Should I prioritize <see cref="Item"/>s or <see cref="Character"/>s and by how much</summary>
    public BehaviorPriority prioritize;
    /// <summary>How much away from the spot before they should move again</summary>
    protected float idealDistanceLeeway = 1;
    /// <summary>By default am I sleeping, inactive, etc.</summary>
    public CharacterState defaultState;
    
    protected bool isAware = false;
    protected GameObject awareOf;

    protected LayerMask obstacleLayer;
    /// <summary>Range that enemy looks for other focuses in</summary>
    public float range;
    public float meleeDamage;
    public float meleeRange;
    /// <summary>How long is my attack cooldown timer (sec)</summary>
    public float cooldown;

    // For AI/Pathfinding
    protected Pathfinding.AIPath ai;
    protected Pathfinding.Seeker seeker;
    protected AIDestinationSetter destinationSetter;
    private Vector3 lastTargetPoint;
    private int checksToPoint = 0;
    private Vector3[] lastLocations = new Vector3[3];
    private int unstuckTries = 0;
    private Collider2D territory;
    private bool drawLinesOfSight = false;
    protected bool drawFocus = true;

    /// <summary>Possible <see cref="Action"/>s for an enemy to take</summary>
    public List<Action> possibleActions = new List<Action>();
    /// <summary>What I'm looking at; fleeing from or following</summary>
    public GameObject focus;
    /// <summary>Position of what I'm focused on based on its collider</summary>
    public Vector3 focusPos;
    /// <summary>Position of where I'm moving to because of focus and state</summary>
    public Vector3 targetPos;

    [System.NonSerialized]
    public bool dead = false;
    [SerializeField]
    protected bool cooldownAttack = false;
    /// <summary>Script to spawn item if there is one attached</summary>
    protected MonoBehaviour itemSpawner;
    [System.NonSerialized]
    public Collider2D collider2d;
    [System.NonSerialized]
    public Rigidbody2D rb;
    protected System.Random random;

    /// <summary>Initializes enemy data</summary>
    void Start()
    {
        lastLocations[0] = transform.position;
        lastLocations[1] = transform.position;
        lastLocations[2] = transform.position;
        random = GameController.seededRandom;
        foreach (Effect e in attackEffects) {
            if (e.particles == null) e.particles = EffectController.instance.GetParticles(e.type, e.level);
        }
        // player = GameObject.FindGameObjectWithTag("Player");
        itemSpawner = gameObject.GetComponent<ItemSpawner>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        obstacleLayer = LayerMask.GetMask("Wall");
        SetupAI();
    }

    /// <summary>Calls <see cref="Start"/></summary>
    public void EnemyStart() {Start();}

    /// <summary>Looks at surroundings and determines what to do</summary>
    protected void EnemyUpdate()
    {
        if (dead || currState == CharacterState.inactive) return;
        possibleActions.Clear();
        RegenMana();
        if (awareOf != null)
        {
        //     CheckBehavior(awareOf, new Relationship(awareOf.name, 0, defaultBehavior));
        }
        if (GetComponent<TeamingEnemy>() != null)
        {
            TeamingEnemy teamer = GetComponent<TeamingEnemy>();
            if (teamer.AnyAwareOf != null) 
            {
                // if (teamer is CirclerEnemy) CheckBehavior(teamer.AnyAwareOf, new Relationship(awareOf.name, 0, defaultBehavior, (teamer as CirclerEnemy).));
                CheckBehavior(teamer.AnyAwareOf, new Relationship(awareOf.name, 0, defaultBehavior));
            }
        }
        foreach (Collider2D collider in GetPossibleFocuses(transform.position, range))
        {
            // If Enemy sees another Character, it will look at its relationship with that Character,
            // which has combination of when flee, when approach, when attack and check if any
            // requirements are met. If there isn't a relationship it will just do its default behavior. 
            Character character = collider.gameObject.GetComponent<Character>();
            if (character != null && character.currState != CharacterState.inactive && collider != GetComponent<Collider2D>())
            {
                float proximityPriority = 1.01f - (.1f + Vector2.Distance(transform.position, character.transform.position)) / (.1f + range);
                float lineOfSightPriority = CanSeeTarget(character.transform.position, obstacleLayer) ? 1 : .2f;
                bool relationshipFound = false;
                // Look at relationship with each character
                foreach (Relationship r in relationships)
                {
                    if (r.name == character.species)
                    {
                        relationshipFound = true;
                        CheckBehavior(character, new Relationship(character.species, Mathf.RoundToInt(r.priority * proximityPriority * lineOfSightPriority), r.behavior));
                        break;
                    }
                }
                if (!relationshipFound) {
                    CheckBehavior(character, new Relationship(character.species, Mathf.RoundToInt(2 * proximityPriority * lineOfSightPriority), defaultBehavior));
                }
            } 
            else if (collider.gameObject.GetComponent<ItemController>() && currState != CharacterState.inactive)
            {  
                ItemController item = collider.gameObject.GetComponent<ItemController>();
                // Debug.Log("Item found: " + item.item.name);
                float proximityPriority = 1.01f - (.1f + Vector2.Distance(transform.position, item.transform.position)) / (.1f + range);
                float lineOfSightPriority = CanSeeTarget(item.transform.position, obstacleLayer) ? 1 : .2f;
                bool relationshipFound = false;
                // Could change this to use ItemRelationships
                // Look at relationship with each item
                foreach (Relationship r in relationships)
                {
                    if (r.name == item.name)
                    {
                        relationshipFound = true;
                        CheckBehavior(item, new Relationship(item.item.name, Mathf.RoundToInt(r.priority * proximityPriority * lineOfSightPriority), r.behavior));
                        break;
                    }
                }
                if (!relationshipFound) 
                {
                    CheckBehavior(item, new Relationship(item.item.name, Mathf.RoundToInt(2 * proximityPriority * lineOfSightPriority), itemDefaultBehavior));
                }
            }
        }
        DecideAction();
        switch(currState)
        {
            case(CharacterState.inactive):
                if (ai) ai.enabled = false; 
                return;
            case(CharacterState.wander): 
                if (ai) ai.enabled = true; 
                break;
            case(CharacterState.follow):
                if (ai) ai.enabled = true; 
                break;
            case(CharacterState.flee):
                if (ai) ai.enabled = true; 
                break;
            case(CharacterState.attack):
                break;
            case(CharacterState.die):
                if (ai) ai.enabled = false; 
                break;
        }
    }

    /// <summary>Returns colliders around a position</summary>
    /// <param name="pos">Position to look around</param>
    /// <param name="range">Radius around position to check</param>
    /// <returns>Array of colliders in that radius</returns>
    public virtual Collider2D[] GetPossibleFocuses(Vector3 pos, float range)
    {return Physics2D.OverlapCircleAll(transform.position, range);}

    /// <summary>Alerts enemy to presence of attacker for 3 seconds because it was hit</summary>
    public IEnumerator Alert(GameObject attacker)
    {
        isAware = true;
        awareOf = attacker;
        yield return new WaitForSeconds(3);
        isAware = false;
    }

    /// <summary>Just move around aimlessly</summary>
    protected abstract void Wander();
    /// <summary>Attack, attempt to damage to the focus</summary>
    protected abstract void Attack();
    /// <summary>Move relative to focus to maintain distance</summary>
    protected abstract void Follow();
    /// <summary>Move away from the focus</summary>
    public virtual void Flee() 
    {
        // Make the enemy face the target
        int totalChecks = 16;
        Vector3 bestPoint = focusPos;
        float farthestDistance = 0;
        Vector3 newPoint = focusPos;
        // The greater the distCheck, the smaller chance of getting cornered.
        int distCheck = 10;
        float angle = 0;
        Vector3 direction;
        Vector3 bestDirection = Vector3.zero;
        for (int i = 0; i < totalChecks; i++)
        {
            angle = i * (360f / totalChecks) * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
            if (Physics2D.Raycast(transform.position, direction, distCheck, obstacleLayer))
            {newPoint = Physics2D.Raycast(transform.position, direction, distCheck, obstacleLayer).point;}
            else
            {newPoint = transform.position + (direction * distCheck);}
            float distance = Vector3.Distance(newPoint, focusPos);
            if (distance > farthestDistance) 
            {
                bestPoint = newPoint;
                farthestDistance = distance;
                bestDirection = direction;

                // Debug.Log("From " + bestPoint + " to " + focusPos + " is a new farthest distance: " + farthestDistance);
            }
        }
        if (drawFocus) Debug.DrawLine(transform.position, bestPoint, Color.magenta);
        // rb.AddForce(bestDirection * moveSpeed * 0.05f, ForceMode2D.Impulse);
        targetPos = bestPoint;
        MoveSmallTowardsPoint(targetPos, moveSpeed, ForceMode2D.Force);
    }
    
    /// <summary>
    /// Destroys enemy and drops item if it has one attached
    /// /// </summary>
    public override void Die()
    {
        // If we don't check if its dead first then it could spawn multiple
        if (itemSpawner && !dead) {gameObject.GetComponent<ItemSpawner>().SpawnItem();}
        dead = true;
        Destroy(gameObject);
    }

    /// <summary>
    /// Returns true if enemy is in its territory or errors if territory collider is null
    /// </summary>
    /// <returns>True if I'm in my territory</returns>
    protected virtual bool InTerritory()
    {
        if (territory == null) 
        {
            Debug.LogError("Need a territory collider for this enemy");
            return false; // Return false if no territory is defined
        }
        
        // Set up a filter that will allow all layers, or you can specify layers to include
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.AllLayers); // Adjust the layer mask as needed
        contactFilter.useTriggers = true; // Include triggers if applicable
        
        // Create an array to hold colliders that overlap
        Collider2D[] results = new Collider2D[1]; // We only need to check if one collider overlaps
        
        // Check if the specified collider overlaps with the territory
        int overlapCount = territory.OverlapCollider(contactFilter, results);
        
        // Return true if the collider is inside the territory, otherwise false
        return overlapCount > 0 && results[0] == collider2d;
    }

    // public void Shoot()
    
    /// <summary>
    /// Moves enemy small distance towards next pathfinding point, don't use on medium/big distances
    /// </summary>
    /// <param name="finalDest">Where we want to end up</param>
    /// <param name="speed">Speed at which to move towards the target</param>
    /// <param name="forceMode">How to move, impulse or force</param>
    public void MoveSmallTowardsPoint(Vector3 finalDest, float speed, ForceMode2D forceMode)
    {
        destinationSetter.target = finalDest;
        Vector3 point = ai.GetNextPoint();
        // Calculate direction towards the point
        Vector2 direction = (point - transform.position);
        // Debug.Log("Move to " + point + " and last one was " + lastTargetPoint + ", " + checksToPoint);
        // Debug.Log("lastLocation: " + lastLocations[2] + " - currLocation: " + transform.position + " = " + Vector3.Distance(lastLocations[2], transform.position));
        if (Vector2.Distance(lastTargetPoint, point) < .01) checksToPoint++;
        else checksToPoint = 0;
        if (checksToPoint > 100 && Vector3.Distance(lastLocations[2], transform.position) < .005 && Vector2.Distance(transform.position, destinationSetter.target) > .6 && unstuckTries < 50) 
        {
            unstuckTries++;
            Vector2 prev = direction;
            if (direction.x == 0) direction.x = 0.01f;
            if (direction.y == 0) direction.y = 0.01f;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) 
            {
                if (unstuckTries % 10 < 5) direction = new Vector2(direction.x/10, direction.y * 10);
                else direction = new Vector2(direction.x/10, -direction.y*10);
            }
            else 
            {
                if (unstuckTries % 10 < 5) direction = new Vector2(direction.x*10, direction.y/10);
                else direction = new Vector2(-direction.x*10, direction.y/10);
            }
            rb.AddForce(direction * (speed * 1f), ForceMode2D.Impulse);
            // Debug.Log("Adding force in direction: " + direction + " Because we want to move in " + prev + ", tries = " + unstuckTries);
        }
        else
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // Apply force towards the player
            rb.AddForce(direction * (speed * 4f), forceMode);
            unstuckTries = Mathf.Max(0, unstuckTries - 10);
        }
        lastLocations[2] = lastLocations[1];
        lastLocations[1] = lastLocations[0];
        lastLocations[0] = transform.position;
        lastTargetPoint = point;
    }

    /// <summary>Starts cooldown until enemy can attack again</summary>
    protected IEnumerator Cooldown()
    {
        cooldownAttack = true;
        yield return new WaitForSeconds(cooldown * attackRateModifier);
        cooldownAttack = false;
    }

    /// <summary>Starts cooldown until enemy can attack again</summary>
    /// <param name="cooldown">How many sec to wait</param>
    protected IEnumerator Cooldown(float cooldown)
    {
        cooldownAttack = true;
        yield return new WaitForSeconds(cooldown);
        cooldownAttack = false;
    }

    /// <summary>Sets up the AI with adjusted penalties based on enemy size</summary>
    protected void SetupAI()
    {
        ai = gameObject.AddComponent<Pathfinding.AIPath>();
        ai.orientation = OrientationMode.YAxisForward;
        ai.maxSpeed = moveSpeed;
        ai.constrainInsideGraph = true;

        destinationSetter = gameObject.GetComponent<Pathfinding.AIDestinationSetter>();
        destinationSetter.target = GameObject.FindGameObjectWithTag("Player").transform.position;

        seeker = gameObject.GetComponent<Pathfinding.Seeker>();
        seeker.drawGizmos = false;
        ai.pickNextWaypointDist = 0.5f;

        // Calculate penalty based on the collider's size
        collider2d = GetComponent<Collider2D>();
        if (collider2d != null)
        {
            // Determine the penalty multiplier based on the width of the collider
            float sizePenalty = Mathf.Max(collider2d.bounds.size.x, collider2d.bounds.size.y);
            int tagNum = 1;
            while (sizePenalty > 0)
            {
                // Apply a scaled penalty for larger colliders (adjust index based on your setup)
                seeker.tagPenalties[tagNum] = Mathf.CeilToInt(Mathf.Pow(sizePenalty, 1.5f) * 200f); // Adjust the multiplier as needed
                sizePenalty -= 0.5f;
                tagNum++;
            }
        }
        else
        {
            Debug.LogWarning("Collider2D not found on enemy. Tag penalties will not be adjusted.");
        }
    }

    /// <summary>
    /// Return whether the enemy can see a target, no obstacles in the way
    /// </summary>
    /// <param name="target">What position to look to</param>
    /// <param name="obstacleLayer">What layers to look for obstacles on</param>
    /// <returns>True if there is clear line of sight</returns>
    public bool CanSeeTarget(Vector3 target, LayerMask obstacleLayer)
    {
        // Raycast from the current object's position to the target
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target - transform.position, Vector3.Distance(transform.position, target), obstacleLayer);
        if (drawLinesOfSight) {
            if (hit.collider == null)
            {
                Debug.DrawLine(transform.position, target, Color.blue);
            }
            else
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
        }
        // If the raycast hits nothing, it means there's a clear line of sight
        return hit.collider == null;
    }

    /// <summary>
    /// Tests whether a certain position can see a target with no obstacles in the way
    /// </summary>
    /// <param name="from">What position to check from</param>
    /// <param name="target">What position to look to</param>
    /// <param name="obstacleLayer">What layers to look for obstacles on</param>
    /// <returns>True if there is clear line of sight</returns>
    public bool CanSeeTarget(Vector3 from, Vector3 target, LayerMask obstacleLayer)
    {
        // Raycast from the specified 'from' position to the target
        RaycastHit2D hit = Physics2D.Raycast(from, target - from, Vector3.Distance(from, target), obstacleLayer);

        if (drawLinesOfSight) {
            if (hit.collider == null)
            {
                Debug.DrawLine(transform.position, target, Color.blue);
            }
            else
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
        }

        // If the raycast hits nothing, it means there's a clear line of sight
        return hit.collider == null;
    }

    /// <summary>MAKE THIS BETTER</summary>
    /// <param name="obstacleLayer">Layers to check for walls in</param>
    /// <returns>True if blocked in 2+ directions</returns>
    public bool IsCornered(LayerMask obstacleLayer)
    {
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        // Track how many sides are blocked
        int blockedCount = 0;

        // Check each direction using raycasts
        foreach (Vector2 direction in directions)
        {
            // Cast a ray in the direction and check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 3f, obstacleLayer);
            
            // If the ray hits something, increment the blocked count
            if (hit.collider != null)
            {
                blockedCount++;
            }
        }
        return blockedCount >= 2;
    }

    /// <summary>
    /// Looks at the Relationship with another Character and determines what Action to add to list
    /// </summary>
    /// <param name="other">Other Character to look at</param>
    /// <param name="r">Relationship with the other Character</param>
    protected void CheckBehavior(Character other, Relationship r)
    {
        if (other.gameObject == gameObject)
        {
            Debug.Log("Ignore self from nearby");
            return;
        }

        float distance = Vector3.Distance(transform.position, other.transform.position);
        // Distances from ideals are used to determine priority of action
        // How far are we from the distance we want to be following to
        float distFromFollowIdeal = distance - r.behavior.followDist;
        // How far are we from the distance we want to be fleeing to (if we want to flee infinitely, we set max as 10)
        float distFromFleeIdeal = r.behavior.fleeDist == float.PositiveInfinity ? r.behavior.fleeDist - distance : 10;

        // If we're so close we should flee (move away)
        bool inFleeDist = r.behavior.fleeDist <= distance + idealDistanceLeeway;
        // If we're so far we should follow (move closer)
        bool inFollowDist = distance >= r.behavior.followDist;

        if (r.behavior.whenFlee.Contains(WhenFlee.Always) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, other.gameObject, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfAttacked) && attackedBy.Contains(other.species) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, other.gameObject, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfImWeaker) && WeakerThan(other) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, other.gameObject, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfImNotHome) && !InTerritory() && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, other.gameObject, distFromFleeIdeal));
        } // Check whether to approach now
        else if (r.behavior.whenFollow.Contains(WhenFollow.Always) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, other.gameObject, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfAttacked) && attackedBy.Contains(other.species) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, other.gameObject, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfImHome) && InTerritory() && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, other.gameObject, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfImStronger) && !WeakerThan(other) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, other.gameObject, distFromFollowIdeal));
        }

        // Check if I can attack too
        if (r.behavior.whenAttack.Contains(WhenAttack.Always))
        {
            willAttack.Add(other.gameObject);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.InTerritory) && !InTerritory())
        {
            willAttack.Add(other.gameObject);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.AttackedFirst) && attackedBy.Contains(other.species))
        {
            willAttack.Add(other.gameObject);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.Cornered) && IsCornered(obstacleLayer))
        {
            willAttack.Add(other.gameObject);
        }
    }

    /// <summary>
    /// Looks at the relationship with an item and determines what Action to add to list
    /// </summary>
    /// <param name="item">ItemController to look at</param>
    /// <param name="r">Relationship with the item</param>
    protected void CheckBehavior(ItemController item, Relationship r)
    {
        float distance = Vector3.Distance(transform.position, item.transform.position);
        // Distances from ideals are used to determine priority of action
        // How far are we from the distance we want to be following to
        float distFromFollowIdeal = distance - r.behavior.followDist;
        // How far are we from the distance we want to be fleeing to (if we want to flee infinitely, we set max as 10)
        float distFromFleeIdeal = r.behavior.fleeDist == float.PositiveInfinity ? r.behavior.fleeDist - distance : 10;

        // If we're so close we should flee (move away)
        bool inFleeDist = r.behavior.fleeDist <= distance + idealDistanceLeeway;
        // If we're so far we should follow (move closer)
        bool inFollowDist = distance >= r.behavior.followDist;

        if (r.behavior.whenFlee.Contains(WhenFlee.Always) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, item.gameObject, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfAttacked) && attackedBy.Contains(item.name) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, item.gameObject, distFromFleeIdeal));
        }
        // else if (r.behavior.whenFlee.Contains(WhenFlee.IfImWeaker) && WeakerThan(item))
        // {
        //     possibleActions.Add(new Action(MovementType.Flee, r.priority, other.gameObject));
        // }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfImNotHome) && !InTerritory() && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, item.gameObject, distFromFleeIdeal));
        } // Check whether to approach now
        else if (r.behavior.whenFollow.Contains(WhenFollow.Always) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, item.gameObject, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfAttacked) && attackedBy.Contains(item.name) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, item.gameObject, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfImHome) && InTerritory() && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, item.gameObject, distFromFollowIdeal));
        }
        // else if (r.behavior.whenFollow.Contains(WhenFollow.IfImStronger) && !WeakerThan(other))
        // {
        //     possibleActions.Add(new Action(MovementType.Follow, r.priority, other.gameObject));
        // }

        // Check if I can attack too
        if (r.behavior.whenAttack.Contains(WhenAttack.Always))
        {
            willAttack.Add(item.gameObject);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.InTerritory) && !InTerritory())
        {
            willAttack.Add(item.gameObject);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.AttackedFirst) && attackedBy.Contains(item.name))
        {
            willAttack.Add(item.gameObject);
        }
        // else if (r.behavior.whenAttack.Contains(WhenAttack.Cornered) && IsCornered(obstacleLayer))
        // {
        //     willAttack.Add(other.gameObject);
        // }
    }

    /// <summary>
    /// Looks at the relationship with an unknown GameObject and determines what Action to add to list
    /// </summary>
    /// <param name="unknown">A GameObject to consider, but we don't know what it is</param>
    /// <param name="r">Relationship with a generic unknown thing</param>
    protected void CheckBehavior(GameObject unknown, Relationship r)
    {
        if (unknown == gameObject) 
        {
            Debug.Log("Ignore self (" + gameObject + ") from alert");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, unknown.transform.position);
        // Distances from ideals are used to determine priority of action
        // How far are we from the distance we want to be following to
        float distFromFollowIdeal = distance - r.behavior.followDist;
        // How far are we from the distance we want to be fleeing to (if we want to flee infinitely, we set max as 10)
        float distFromFleeIdeal = r.behavior.fleeDist == float.PositiveInfinity ? r.behavior.fleeDist - distance : 10;

        // If we're so close we should flee (move away)
        bool inFleeDist = r.behavior.fleeDist <= distance + idealDistanceLeeway;
        // If we're so far we should follow (move closer)
        bool inFollowDist = distance >= r.behavior.followDist;

        if (r.behavior.whenFlee.Contains(WhenFlee.Always) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, unknown, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfAttacked) && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, unknown, distFromFleeIdeal));
        }
        else if (r.behavior.whenFlee.Contains(WhenFlee.IfImNotHome) && !InTerritory() && inFleeDist)
        {
            possibleActions.Add(new Action(MovementType.Flee, r.priority, unknown, distFromFleeIdeal));
        } // Check whether to approach now
        else if (r.behavior.whenFollow.Contains(WhenFollow.Always) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, unknown, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfAttacked) && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, unknown, distFromFollowIdeal));
        }
        else if (r.behavior.whenFollow.Contains(WhenFollow.IfImHome) && InTerritory() && inFollowDist)
        {
            possibleActions.Add(new Action(MovementType.Follow, r.priority, unknown, distFromFollowIdeal));
        }

        // Check if I can attack too
        if (r.behavior.whenAttack.Contains(WhenAttack.Always))
        {
            willAttack.Add(unknown);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.InTerritory) && !InTerritory())
        {
            willAttack.Add(unknown);
        }
        else if (r.behavior.whenAttack.Contains(WhenAttack.AttackedFirst))
        {
            willAttack.Add(unknown);
        }
    }

    /// <summary>Changes state and focus of Enemy based on possible actions from surrounding area</summary>
    protected virtual void DecideAction()
    {
        Action highestPriorityAction = new Action(MovementType.Follow, int.MinValue, gameObject);
        if (possibleActions.Count == 0) 
        {
            currState = CharacterState.wander;
            SetFocus(gameObject);
            return;
        }
        for (int i = 0; i < possibleActions.Count; i++)
        {
            Action action = possibleActions[i];
            switch (prioritize.focusType)
            {
                case FocusPriority.character:
                    // Check if focus is a Character or if the highest priority focus isn't a Character
                    if (action.focus.GetComponent<Character>() != null)
                    {
                        action.priority = action.priority + prioritize.addend;
                        highestPriorityAction = CompareActions(action, highestPriorityAction);
                    } 
                    else if (highestPriorityAction.focus.GetComponent<Character>() == null)
                    {
                        highestPriorityAction = CompareActions(action, highestPriorityAction);
                    }
                    break;
                case FocusPriority.item:
                    // Check if focus is an Item or if the highest priority focus isn't an Item
                    if (action.focus.GetComponent<ItemController>() != null)
                    {
                        action.priority = action.priority + prioritize.addend;
                        highestPriorityAction = CompareActions(action, highestPriorityAction);
                    }
                    else if (highestPriorityAction.focus.GetComponent<ItemController>() == null)
                    {
                        highestPriorityAction = CompareActions(action, highestPriorityAction);
                    }
                    break;
                case FocusPriority.none:
                    highestPriorityAction = CompareActions(action, highestPriorityAction);
                    break;
            }
        }

        if (highestPriorityAction.movementType == MovementType.Follow)
        {
            currState = CharacterState.follow;
        }
        else
        {
            currState = CharacterState.flee;
            // Need to set this up so enemies follow to point and flee if within flee distance but then
            // also need to change the EnemyUpdate system so it factors in distance into whether an 
            // action is considered how it should be
        }
        SetFocus(highestPriorityAction.focus);
    }

    /// <summary>
    /// Returns Action with higher priority by action.priority or if equal, then proximity
    /// </summary>
    /// <param name="action">New Action to compare to the existing highest priority</param>
    /// <param name="highestPriorityAction">Existing highest priority action</param>
    /// <returns>Whichever action has higher priority by priority or if equal, then proximity</returns>
    private Action CompareActions(Action action, Action highestPriorityAction)
    {
        if (highestPriorityAction.priority < action.priority || 
                (highestPriorityAction.priority == action.priority &&
                Vector3.Distance(action.focus.transform.position, transform.position) <
                Vector3.Distance(highestPriorityAction.focus.transform.position, transform.position))) 
        {
            return action;
        }
        return highestPriorityAction;
    }

    /// <summary>Sets this Enemy's focus to the given focus and draws a line if debugging</summary>
    /// <param name="focus">GameObject to focus on</param>
    protected virtual void SetFocus(GameObject focus)
    {
        this.focus = focus;
        if (drawFocus) Debug.DrawLine(transform.position, focus.transform.position, Color.white);
    }
}