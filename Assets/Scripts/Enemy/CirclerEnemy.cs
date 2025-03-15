using System.Collections;
using UnityEngine;

public abstract class CirclerEnemy : TeamingEnemy
{   
    protected bool followCooldown = false;
    public int mySpot = -1;
    private bool resetFocus = true;

    void FixedUpdate()
    {
        if (currState == CharacterState.inactive) return;
        if (!CirclerController.teamEstablished) return;
        if (CirclerController.anyAware)
        {
            awareOf = CirclerController.anyAwareOf;
            // A sound would be cool here
            // StartCoroutine(CirclerController.AlertAll());
            
        }   
        EnemyUpdate();
        focusPos = focus.transform.position;
        if (Vector3.Distance(transform.position, focusPos) <= meleeRange * attackSizeModifier && !cooldownAttack && willAttack.Contains(focus))
            {
                currState = CharacterState.attack;
                Attack();
            }
        switch(currState)
        {
            case(CharacterState.inactive):
                return;
            case(CharacterState.wander): 
                Wander();
                break;
            case(CharacterState.follow):
                if (!followCooldown) StartCoroutine(FrequentFollow(0.05f));

                break;
            case(CharacterState.flee):
                Flee();
                break;
            case(CharacterState.attack):
                // Keep doing attack thing
                break;
            case(CharacterState.die):
                Die();
                break;
        }
    }

    /// <summary>
    /// Activate the Following code every so often to not do it every frame
    /// </summary>
    /// <param name="cooldown">How long to wait until it tries to follow again (in sec)</param>
    protected IEnumerator FrequentFollow(float cooldown)
    {
        followCooldown = true;
        Follow();
        yield return new WaitForSeconds(cooldown);
        followCooldown = false;
    }

    /// <summary>Follow the target in a pack, surrounding the target or flee if there's only one left</summary>
    protected override void Follow() 
    {
        // Make the enemy face the focus (or not)
        Vector3 direction = (focusPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        CirclerController.CalculateSpot(this, focusPos, obstacleLayer, transform.position);
        targetPos = CirclerController.CalculateTargetPosition(myNumber, focusPos);
        ai.SetDestination(targetPos);
        // MoveSmallTowardsPoint(targetPos, moveSpeed * 5, ForceMode2D.Force);
    }

    protected override void Wander() 
    {
        // Implement the wandering logic
    }

    /// <summary>
    /// Sets the focus for this circler enemy and shares it with other team members
    /// </summary>
    /// <param name="focus">GameObject to focus on</param>
    protected override void SetFocus(GameObject focus)
    {
        if (CirclerController.focus == null || resetFocus)
        {
            CirclerController.focus = focus;
            StartCoroutine(ResetFocusTimer(3));
        }
        
        this.focus = CirclerController.focus;
        if (drawFocus) 
        {
            Color lineColor = currState == CharacterState.flee ? Color.cyan : Color.white;
            Debug.DrawLine(transform.position, this.focus.transform.position, lineColor);
        }
    }

    private IEnumerator ResetFocusTimer(float time)
    {
        resetFocus = false;
        yield return new WaitForSeconds(time);
        resetFocus = true;
    }


    protected class CirclerController : TeamController{
        /// <summary>How far away from the target the enemy should be</summary>
        public static float circleRadius = 5f;
        // public static float maxGapDistance = 2f; // Maximum distance allowed between enemies
        /// <summary>Total number of possible spots around the circle</summary>
        public static int totalSpots = 18; 
        /// <summary>How many checks per enemy have to pass before we redo the spot assigning</summary>
        private const int TIME_AT_SPOT = 5;
        /// <summary>How many checks left until we reassign the spots</summary>
        private static int checksUntilReset = 0;
        /// <summary>Which spots in circle are occupied, true if occupied</summary>
        private static bool[] occupiedSpots = new bool[totalSpots];

        public static GameObject focus;

        /// <summary>
        /// Calculates the index of the place around the circle an enemy should go to, places them in enemySpots array
        /// </summary>
        /// <param name="enemyIndex">Index/number of enemy in pack</param>
        /// <param name="targetPosition">Position target is currently at</param>
        /// <param name="obstacleLayer">Layer of obstacles to avoid (walls)</param>
        /// <param name="enemyPosition">Position enemy is currently at</param>
        public static void CalculateSpot(CirclerEnemy caller, Vector3 targetPosition, LayerMask obstacleLayer, Vector3 enemyPosition)
        {
            if (occupiedSpots.Length <= caller.myNumber)
            {
                totalSpots++;
                occupiedSpots = new bool[totalSpots];
                checksUntilReset = 0;
                Debug.Log("Pack size > number of spots available: " + (totalSpots - 1));
            }
            // Debug.Log(string.Join(", ", occupiedSpots));
            if ((checksUntilReset <= 0 || caller.mySpot == -1)) 
            {
                int spot = -1;
                if (checksUntilReset <= 0) 
                {
                    ResetOccupiedSpots();
                    checksUntilReset = team.Count * TIME_AT_SPOT;
                }
                float closestDistance = float.MaxValue;

                // Calculate potential positions on the circle
                for (int i = 0; i < totalSpots; i++) 
                {
                    if (occupiedSpots[i]) continue; // Skip if spot is occupied

                    float angle = i * (360f / totalSpots) * Mathf.Deg2Rad;
                    Vector3 position = targetPosition + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleRadius;
                    // Debug.Log("Position " + position + CanSeeTarget(position, targetPosition, obstacleLayer) + " can see target");
                    if (!CanSeeTarget(position, targetPosition, obstacleLayer)) continue;

                    // Calculate the distance from the enemy to the potential position
                    float distance = Vector3.Distance(enemyPosition, position);

                    // If this is the closest position so far, update the target
                    if (distance < closestDistance) 
                    {
                        closestDistance = distance;
                        spot = i;
                    }
                }
                // If a spot is selected, mark it with whoever has it
                if (spot != -1) 
                {
                    occupiedSpots[spot] = true;
                    caller.mySpot = spot;
                    // Debug.Log("Enemy #" + enemyIndex + " chose spot #" + enemySpots[enemyIndex]);
                } else 
                { // Otherwise increase capacity of circle
                    totalSpots++;
                    occupiedSpots = new bool[totalSpots];
                    checksUntilReset = 0;
                    Debug.Log("Pack size > number of spots available: " + (totalSpots - 1));
                    CalculateSpot(caller, targetPosition, obstacleLayer, enemyPosition);
                }
            }
            else {
                checksUntilReset--;
            }
        }

        /// <summary>
        /// Calculates the position an enemy should go to based on its place in the circle
        /// </summary>
        /// <param name="enemyIndex">Index/number of enemy in pack</param>
        /// <param name="targetPosition">Position target is currently at</param>
        /// <param name="obstacleLayer">Layer of obstacles to avoid (walls)</param>
        /// <param name="enemyPosition">Position enemy is currently at</param>
        public static Vector3 CalculateTargetPosition(int spot, Vector3 targetPosition) 
        {
            float angle = spot * (360f / totalSpots) * Mathf.Deg2Rad;
            return targetPosition + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleRadius;
        }
        
        /// <summary>
        /// Tests whether a certain position can see a target with no obstacles in the way
        /// </summary>
        /// <param name="from">What position to check from</param>
        /// <param name="target">What position to look to</param>
        /// <param name="obstacleLayer">What layers to look for obstacles on</param>
        /// <returns>True if there is clear line of sight</returns>
        public static bool CanSeeTarget(Vector3 from, Vector3 target, LayerMask obstacleLayer)
        {
            Collider2D[] obstacles = Physics2D.OverlapCircleAll(from, circleRadius, obstacleLayer);
            foreach (var obstacle in obstacles)
            {
                if (Vector3.Distance(obstacle.transform.position, from) < 0.2f)
                {
                    return false;
                }
            }
            // Raycast from the specified 'from' position to the target
            RaycastHit2D hit = Physics2D.Raycast(from, target - from, Vector3.Distance(from, target), obstacleLayer);

            if (hit.collider == null)
            {
                Debug.DrawLine(from, target, Color.blue);
            }
            else
            {
                Debug.DrawLine(from, hit.point, Color.red);
            }
            
            // If the raycast hits nothing, it means there's a clear line of sight
            return hit.collider == null;
        }

        /// <summary>Resets the arrays of occupied spots so the enemies will redo their searches</summary>
        public static void ResetOccupiedSpots() 
        {
            for (int i = 0; i < totalSpots; i++) 
            {
                occupiedSpots[i] = false;
            }
            foreach (TeamingEnemy teamer in team) {((CirclerEnemy)teamer).mySpot = -1;}
        }
    }
}