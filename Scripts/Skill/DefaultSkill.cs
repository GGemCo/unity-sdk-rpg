using System.Collections;
using System.Collections.Generic;
using GGemCo.Scripts.Characters;
using GGemCo.Scripts.Configs;
using GGemCo.Scripts.Effect;
using GGemCo.Scripts.Scenes;
using GGemCo.Scripts.TableLoader;
using GGemCo.Scripts.Utils;
using UnityEngine;

namespace GGemCo.Scripts.Skill
{
    /// <summary>
    /// 스킬 컨트롤러에서 생성한 스킬
    /// 데미지
    ///     즉시 적용하는 데미지 처리
    ///     tick time 당 데미지 처리
    /// 어펙트
    ///     데미지 받으면 발동
    ///     즉시 발동
    ///     어펙트는 개별로 작동하기 때문에, 자체적으로 데미지 처리 
    /// 이펙트
    ///     이펙트는 지정된 시간동안 보여주는 기능만. 오로지 그래픽적 기능만. 
    ///     타겟에 생성
    ///     범위에 생성
    ///     데미지 콜백 필요
    /// </summary>
    public class DefaultSkill : MonoBehaviour
    {
        // 사용하는 캐릭터
        private CharacterBase attacker;
        // 스킬 적용 대상
        private CharacterBase target;
        private float tickTime;
        
        private Coroutine coroutineDamageByTickTime;
        private PolygonCollider2D polyCollider2D;
        private CapsuleCollider2D capsuleCollider2D;
        private Vector3 direction;
        private DefaultEffect arrowDefaultEffect;
        
        private StruckTableSkill struckTableSkill;
        private TableEffect tableEffect;

        public void Initialize(CharacterBase character, int skillUid, int skillLevel)
        {
            struckTableSkill = TableLoaderManager.Instance.TableSkill.GetDataByUidLevel(skillUid, skillLevel);
            attacker = character;
            tableEffect = TableLoaderManager.Instance.TableEffect;
 
            if (struckTableSkill.Duration > 0)
            {
                StartCoroutine(RemoveEffectDuration(struckTableSkill.Duration));
            }
            ComponentController.AddRigidbody2D(gameObject);
        }
        private void Start()
        {
            target = SceneGame.Instance.player.GetComponent<CharacterBase>();
            
            // TargetType 이 Fixed 일때는 Target 에 적중했을때 발동
            // Range 일 경우는 범위에 Target 이 있을때 발동
            //      TickTime 이 있을 경우, 범위 안에 있는 Target 에게 TickTime 마다 데미지 적용 
            // 스킬 target 별 처리 
            if (struckTableSkill.Target == SkillConstants.Target.Player)
            {
            }
            else if (struckTableSkill.Target == SkillConstants.Target.Monster)
            {
                // 스킬 사용 거리 가져오기
                // 사용 거리 안에 있는 몬스터 찾기
                target = SceneGame.Instance.mapManager.GetNearByMonsterDistance(struckTableSkill.Distance);
            }

            if (target == null)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("타겟이 없습니다.");
                return;
            }
            
            // 이펙트 처리
            if (struckTableSkill.EffectUid > 0)
            {
                var effectinfo = tableEffect.GetDataByUid(struckTableSkill.EffectUid);
                GameObject prefab = tableEffect.GetPrefab(struckTableSkill.EffectUid);
                if (prefab == null) return;

                // 범위 공격일 때, 타원형 콜라이더를 생성하고 충돌체크를 한다.
                if (struckTableSkill.TargetType == SkillConstants.TargetType.Range && struckTableSkill.DamageRange > 0)
                {
                    float effectSize = effectinfo.Width * prefab.transform.localScale.x; // 이펙트 크기 (고정)
                    float spawnRadius = struckTableSkill.DamageRange; // 이펙트 생성 반경
                    
                    // 타원형
                    float radiusX = spawnRadius; // 가로 반경 (a)
                    float radiusY = spawnRadius/2f; // 세로 반경 (b)
                    float currentRadiusX = effectSize; // 현재 가로 반경
                    float currentRadiusY = effectSize * (radiusY / radiusX); // 현재 세로 반경 (비율 유지)
                    while (currentRadiusX <= radiusX)
                    {
                        int count = Mathf.RoundToInt((2 * Mathf.PI * currentRadiusX) / effectSize); // 해당 타원 둘레에 배치할 개수 계산
                    
                        for (int i = 0; i < count; i++)
                        {
                            float angle = (i / (float)count) * 360f; // 각도 계산
                            float radian = angle * Mathf.Deg2Rad;
                    
                            // 타원 공식 적용
                            float posX = Mathf.Cos(radian) * currentRadiusX;
                            float posY = Mathf.Sin(radian) * currentRadiusY;
                    
                            posX = Random.Range(posX - 10, posX + 10);
                            posY = Random.Range(posY - 10, posY + 10);
                            
                            Vector3 spawnPosition = target.transform.position + new Vector3(posX, posY, 0);

                            DefaultEffect defaultEffect = EffectManager.CreateEffect(struckTableSkill.EffectUid);
                            defaultEffect.SetDuration(struckTableSkill.Duration);
                            defaultEffect.gameObject.transform.position = spawnPosition;
                        }
                    
                        // 다음 타원 크기 증가
                        currentRadiusX += effectSize;
                        currentRadiusY += effectSize * (radiusY / radiusX);
                    }
                    
                    // skill 오브젝트에 polygon 콜라이더 추가하기
                    int vertexCount = 20; // 꼭짓점 개수 (값이 클수록 부드러움)
                    Vector2[] points = new Vector2[vertexCount];
                    float diffX = 10f;
                    float diffY = 10f;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        float angle = (i / (float)vertexCount) * Mathf.PI * 2;
                        float x = Mathf.Cos(angle) * (radiusX - diffX);
                        float y = Mathf.Sin(angle) * (radiusY - diffY);
                        points[i] = new Vector2(x, y);
                    }
                    Vector2 offset = Vector2.zero;
                    polyCollider2D = ComponentController.AddPolygonCollider2D(gameObject, true, offset, points);
                    
                    if (struckTableSkill.DamageValue > 0 && struckTableSkill.TickTime > 0)
                    {
                        coroutineDamageByTickTime = StartCoroutine(DamageByTickTime());
                    }
                    transform.position = target.transform.position;
                }
                // 범위 공격이 아닐때, 이펙트 테이블에 있는 콜라이더 크기로 캡슐 콜라이더를 만들고 충돌 체크를 한다
                else
                {
                    arrowDefaultEffect = EffectManager.CreateEffect(struckTableSkill.EffectUid);
                    arrowDefaultEffect.gameObject.transform.SetParent(gameObject.transform);
                    
                    // 날아가지 않는 이펙트는 바로 타겟 위치로
                    if (struckTableSkill.EffectMoveSpeed <= 0)
                    {
                        transform.position = target.transform.position;
                    }
                    else
                    {
                        transform.position = attacker.transform.position + new Vector3(0, attacker.GetHeightByScale() / 2f, 0);
                    }
                    
                    // 방향 계산 (목표 지점 - 현재 위치)
                    Vector3 targetPosition = target.transform.position + new Vector3(0, target.GetHeightByScale() / 2f, 0);
                    direction = (targetPosition - transform.position).normalized;

                    // 방향에 따라 이펙트 좌우 반전
                    float dirX = GetScaleByDirection();
                    arrowDefaultEffect.SetDirection(dirX);
                    Vector2 directionByTarget = target.transform.position - transform.position;
                    // 방향에 따라 rotation 처리 
                    arrowDefaultEffect.SetRotation(directionByTarget, direction);
                    // 이펙트가 destroy 되었을때 콜백
                    arrowDefaultEffect.OnEffectDestroy += OnArrowEffectDestroy;

                    // 범위 공격이 아닐때, 이펙트 collider 로 충돌 체크를 한다
                    Vector2 size = new Vector2(effectinfo.ColliderSize.x * arrowDefaultEffect.transform.localScale.x, effectinfo.ColliderSize.y * arrowDefaultEffect.transform.localScale.y);
                    Vector2 offset = Vector2.zero;
                    capsuleCollider2D = ComponentController.AddCapsuleCollider2D(gameObject, true, offset, size);
                }
            }
            
            // 스킬 target type 별 처리 
            if (struckTableSkill.TargetType == SkillConstants.TargetType.Fixed)
            {
                // 어펙트 처리
                if (struckTableSkill.AffectUid > 0)
                {
                    target.AddAffect(struckTableSkill.AffectUid);
                }
            }
            else if (struckTableSkill.TargetType == SkillConstants.TargetType.Range)
            {
                StartCoroutine(AffectByTickTimeOnce());
            }
            
            // 스킬이 발사되면 마력 사용하기
            attacker.MinusMp(struckTableSkill.NeedMp);
        }

        private List<CharacterBase> GetMonsterInCollider()
        {
            List<CharacterBase> characterBases = new List<CharacterBase>();
            if (polyCollider2D == null) return characterBases;
            
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true; // 트리거도 감지하도록 설정
            Collider2D[] results = new Collider2D[100]; // 최대 10개까지 충돌 감지 (필요시 증가 가능)
            // int hitCount = polyCollider2D.OverlapCollider(filter, results);
            int hitCount = Physics2D.OverlapCollider(polyCollider2D, filter, results);

            if (hitCount > 0)
            {
                foreach (Collider2D col in results)
                {
                    if (col == null) continue;
                
                    // 몬스터 태그가 있는 오브젝트인지 확인
                    if (col.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
                    {
                        CharacterHitArea characterHitArea = col.GetComponent<CharacterHitArea>();
                        if (characterHitArea != null)
                        {
                            characterBases.Add(characterHitArea.target);
                        }
                    }
                }
            }

            return characterBases;
        } 

        private IEnumerator AffectByTickTimeOnce()
        {
            if (polyCollider2D == null) yield break;
            
            // 충돌 감지를 확실하게 하기 위해 한 프레임 대기
            yield return null; 
            
            List<CharacterBase> characterBases = GetMonsterInCollider();
            foreach (var character in characterBases)
            {
                if (struckTableSkill.AffectUid <= 0) continue;
                character.AddAffect(struckTableSkill.AffectUid);
            }
            yield return null;
        }
        private IEnumerator DamageByTickTime()
        {
            if (attacker == null) yield break;
            if (polyCollider2D == null) yield break;
            
            // 충돌 감지를 확실하게 하기 위해 한 프레임 대기
            yield return null; 
            
            while (gameObject != null)
            {
                List<CharacterBase> characterBases = GetMonsterInCollider();
                foreach (var character in characterBases)
                {
                    character.TakeDamage(struckTableSkill.DamageValue, attacker.gameObject, struckTableSkill.DamageType);
                }
                yield return new WaitForSeconds(struckTableSkill.TickTime);
            }
        }
        private IEnumerator RemoveEffectDuration(float f)
        {
            yield return new WaitForSeconds(f);
            if (coroutineDamageByTickTime != null)
            {
                StopCoroutine(coroutineDamageByTickTime);
                coroutineDamageByTickTime = null;
            }
            Destroy(gameObject);
        }
        private void Update()
        {
            if (target == null) return;
            if (struckTableSkill.Target == SkillConstants.Target.Player &&
                struckTableSkill.TargetType == SkillConstants.TargetType.Fixed)
            {
                transform.position = attacker.transform.position;
                return;
            }
            if (struckTableSkill.EffectMoveSpeed <= 0) return;
            
            // 이펙트를 타겟 방향으로 이동
            transform.position += direction * (struckTableSkill.EffectMoveSpeed * Time.deltaTime);
        }
 
        /// <summary>
        /// 방향에 따른 X 축 스케일 반환
        /// </summary>
        private int GetScaleByDirection()
        {
            return direction.x >= 0 ? -1 : 1;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag(ConfigTags.GetValue(ConfigTags.Keys.Monster)))
            {
                // tick time 이 있으면 코루틴에서 데미지 처리를 한다.
                if (struckTableSkill.TickTime > 0) return;
                
                CharacterHitArea characterHitArea = collision.GetComponent<CharacterHitArea>();
                if (characterHitArea == null || characterHitArea.target != target) return;
                characterHitArea.target.TakeDamage(struckTableSkill.DamageValue, attacker.gameObject, struckTableSkill.DamageType);
                
                if (struckTableSkill.Duration > 0) return;
                target = null;
                if (arrowDefaultEffect == null) return;
                arrowDefaultEffect.SetEnd();
                
            }
        }

        private void OnArrowEffectDestroy()
        {
            Destroy(gameObject);
        }
    }
}