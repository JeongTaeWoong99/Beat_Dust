## 📑 목차
- [📋 개요](#개요)
- [✨ 주요 기능](#주요-기능)
- [🎯 핵심 담당 기능](#핵심-담당-기능)
- [🎬 인게임 사진](#인게임-사진)
- [🏗️ 프로젝트 구조](#프로젝트-구조)
- [🔗 관련 링크](#관련-링크)
- [🛠 기술 스택](#기술-스택)
- [🏗 아키텍처](#아키텍처)

---

## 📋 개요
<table>
  <tr>
    <td>
      <table>
        <tr><td>기간</td><td>2025.07.18 ~ 2025.07.20  + 1개월</td></tr>
        <tr><td>인원</td><td>5명(기획 1, 클라이언트 2, 아트 2)</td></tr>
        <tr><td>역할</td><td>클라이언트</td></tr>
        <tr><td>도구</td><td>UNITY, C#</td></tr>
        <tr><td>타겟 기기</td><td>PC</td></tr>
        <tr><td>참여 활동</td><td>2025 넥슨 재밌넥(최우수상)</td></tr>
      </table>
    </td>
    <td style="vertical-align: top; padding-left: 20px;">
      <img src="https://github.com/user-attachments/assets/46679bf3-8221-4881-9aef-6083580fb9cf" width="200"/>
    </td>
  </tr>
</table>

몰려오는 먼지들을 박자에 맞춰 물 청소하는 리듬 액션 게임입니다.

음악을 좋아하는 청소부 'Dustin'이 되어, 몰려오는 먼지 더미들을 씻어내야 합니다!

넥슨 '재밌넥'에서 2박 3일간 제작된 프로젝트로, 이후 폴리싱 작업을 거쳐 무료 게임으로 출시하였습니다.

---

## ✨ 주요 기능

### 🎵 리듬 게임 시스템
- **박자 동기화** : 음악의 BPM에 맞춰 적 생성 및 공격 타이밍 동기화
- **타격 판정** : 노드 위치에 따른 판정 시스템
- **노트 생성** : 음악 패턴에 따른 노트 생성 및 관리

### 🎮 게임플레이
- **플레이어 제어** : 마우스와 방향키 기반 직관적인 조작 시스템
- **적 AI** : 패턴에 따라 이동하는 몬스터 시스템
- **난이도 조절** : 다양한 난이도의 음악 선택 가능

### 💾 데이터 관리
- **점수 저장** : 로컬 저장을 통한 최고 점수 기록
- **랭킹 시스템** : 플레이 기록 관리 및 순위 표시
- **설정 저장** : 게임 설정 및 사용자 데이터 관리

### 🎨 UI/UX
- **튜토리얼** : 초보자를 위한 안내 시스템
- **피드백** : 타격 시 시각/청각적 피드백 제공
- **씬 전환** : 부드러운 로딩 화면 및 씬 전환

---

## 🎯 핵심 담당 기능

### 1️⃣ AudioDSP 기반 비트 동기화 시스템

#### 📌 해결하고자 한 문제
Unity의 고질적인 문제인 **사운드 스레드 독립 실행**으로 인해 `Time.timeScale`과 사운드 재생 시간이 어긋나는 현상을 해결하고, 정확한 비트 박자에 맞춰 게임 플레이를 구현하고자 했습니다.

#### 🔧 구현 방법
`AudioSettings.dspTime`을 활용하여 모든 게임 진행 로직(사운드 시작/노드 이동/노드 생성)을 사운드의 비트 타이밍에 정확하게 동기화했습니다.

#### 🎯 핵심 구현 내용

**① AudioSyncManager : 음악 시작 시간 동기화 (AudioSyncManager.cs:66-105)**
```csharp
public void PrepareGame()
{
    gameStartTime = AudioSettings.dspTime; // 게임 시작 시간 기록

    // 첫 노드가 중앙에 도착하는 시간 계산
    float  travelTime           = distance / nodeSpeed;
    double firstNodeArrivalTime = gameStartTime + travelTime;

    // 음악 시작 시간 = 첫 노드 도착 시간 + 한 비트
    songStartTime = firstNodeArrivalTime + secondsPerBeat;
    nextBeatTime  = songStartTime + secondsPerBeat;

    // dspTime 기반으로 음악 예약 재생
    audioSource.PlayScheduled(songStartTime);
}
```
- **dspTime 기반 시간 계산** : 게임 시작 시점, 노드 도착 시간, 음악 시작 시간을 모두 `AudioSettings.dspTime`으로 계산
- **PlayScheduled** : 정확한 시간에 음악이 재생되도록 예약

**② GameManager : dspTime 기반 게임 진행 (GameManager.cs:136-154)**
```csharp
void Update()
{
    if (AudioSyncManager.instance.musicStarted)
    {
        // 음악 진행 시간 = 현재 dspTime - 음악 시작 dspTime - 일시정지 시간
        double musicProgressTime = AudioSettings.dspTime
            - AudioSyncManager.instance.PauseDelayTime
            - AudioSyncManager.instance.SongStartTime;

        // 남은 시간 계산 (음악 진행 시간 기반)
        float targetTime = musicTotalLength - (float)musicProgressTime;
        RemainTime = Mathf.Lerp(RemainTime, targetTime, Time.deltaTime * 2f);
    }
}
```
- **음악 진행 시간 추적** : `dspTime` 차이로 정확한 음악 진행 시간 계산
- **일시정지 보정** : `PauseDelayTime`을 빼서 일시정지 시간을 제외한 실제 진행 시간 계산

**③ RhythmNode : dspTime 기반 노드 이동 (RhythmNode.cs:48-93)**
```csharp
void Update()
{
    // 현재 오디오 시간 기준으로 이동
    double currentAudioTime = AudioSettings.dspTime;
    double timeToTarget     = targetHitTime + AudioSyncManager.instance.PauseDelayTime
                          - currentAudioTime;

    // 거리와 시간을 이용해 정확한 위치 계산
    float totalDistance = Vector3.Distance(startPosition, targetPosition);
    float totalTime     = totalDistance / moveSpeed;
    double elapsedTime  = totalTime - timeToTarget;
    float progress      = Mathf.Clamp01((float)(elapsedTime / totalTime));

    // 위치 업데이트
    transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
}
```
- **실시간 위치 계산** : 목표 도착 시간(`targetHitTime`)과 현재 `dspTime` 차이로 진행도(`progress`) 계산
- **일시정지 대응** : `PauseDelayTime`을 더해 일시정지 시간만큼 목표 시간을 연장

#### ✨ 달성 효과
미묘한 타이밍 오차 없이 사운드의 비트에 **정확히 맞춰** 게임을 진행할 수 있게 되어, 리듬 게임의 핵심인 '비트 플레이'의 몰입도와 재미를 높였습니다.

---

### 2️⃣ Unity Editor 커스텀을 통한 레벨 디자인 도구 개발

#### 📌 해결하고자 한 문제
기획자가 **위/아래/왼쪽/오른쪽 4방향**의 패턴 데이터(각 11자리 문자열)를 직접 수정하고 테스트할 수 있는 직관적인 환경이 필요했습니다.

#### 🔧 구현 방법
`CustomPropertyDrawer`를 활용하여 문자열 패턴을 **시각적인 버튼 그리드**로 표현하고, 클릭만으로 편집 가능하도록 커스텀 에디터를 제작했습니다.

#### 🎯 핵심 구현 내용

**① ScriptableObject 기반 레벨 데이터 구조 (LevelData.cs:5-32)**
```csharp
[CreateAssetMenu(fileName = "Level Data", menuName = "Scriptable Object/Level Data")]
public class LevelData : ScriptableObject
{
    public int              level;                  // 레벨 번호
    public AudioClip        audioClip;              // 음악
    public float            soundBeat;              // BPM
    public int              createAndMoveCountBeat; // 패턴 생성 간격
    public List<StringData> stringData;             // 패턴 데이터 리스트
}

[Serializable]
public class StringData
{
    public string upData;      // 위쪽 패턴 (11자리)
    public string downData;    // 아래쪽 패턴 (11자리)
    public string leftData;    // 왼쪽 패턴 (11자리)
    public string rightData;   // 오른쪽 패턴 (11자리)
}
```

**② CustomPropertyDrawer : 시각적 패턴 편집기 (StringDataDrawer.cs:74-132)**
```csharp
private Rect DrawPatternRow(Rect rect, string label, SerializedProperty stringProperty, bool isVertical)
{
    // 11개의 버튼을 그리드로 표시
    for (int i = 0; i < PATTERN_LENGTH; i++)
    {
        string currentPattern = stringProperty.stringValue;
        int currentValue = currentPattern[i] - '0'; // 0~4 값 추출

        // 가로/세로 배치 선택
        Rect buttonRect = isVertical
            ? new Rect(x, y + i * (SIZE + SPACING), SIZE, SIZE)  // 세로
            : new Rect(x + i * (SIZE + SPACING), y, SIZE, SIZE); // 가로

        // 값에 따라 색상 변경 (1~4: 초록, 0: 빨강)
        GUI.backgroundColor = (currentValue >= 1) ? Color.green : Color.red;

        // 클릭 시 값 순환 (0→1→2→3→4→0)
        if (GUI.Button(buttonRect, currentValue.ToString()))
        {
            int newValue = (currentValue + 1) % 5;
            char[] patternChars = currentPattern.ToCharArray();
            patternChars[i] = (char)('0' + newValue);
            stringProperty.stringValue = new string(patternChars);
        }
    }
}
```

**③ 복사/붙여넣기 지원 (StringDataDrawer.cs:138-192)**
```csharp
// 세로 패턴용 멀티라인 텍스트 영역
if (isVertical)
{
    // 패턴을 멀티라인으로 변환 (예: "01234" → "0\n1\n2\n3\n4")
    string multilinePattern = string.Join("\n", pattern.ToCharArray());

    // 텍스트 영역으로 편집 가능
    string newValue = EditorGUI.TextArea(rect, multilinePattern, customStyle);

    // 입력값 검증 후 패턴에 반영
    if (newValue != multilinePattern)
    {
        string[] lines = newValue.Split('\n');
        string newPattern = "";
        for (int i = 0; i < 11; i++)
        {
            char c = (i < lines.Length) ? lines[i][0] : '0';
            newPattern += (c >= '0' && c <= '4') ? c : '0';
        }
        stringProperty.stringValue = newPattern;
    }
}
```

#### ✨ 달성 효과
- **시각적 편집** : 문자열 대신 버튼 클릭으로 패턴 수정 가능
- **빠른 테스트** : 패턴 변경 후 즉시 게임 실행으로 확인 가능
- **생산성 향상** : 기획자가 코드 없이 레벨 디자인 가능
- **복붙 지원** : 텍스트 영역으로 대량 패턴 입력 가능

---

## 🎬 인게임 사진

<table>
  <tr>
    <td align="center">
      <img width="640" height="360" alt="메인 화면" src="https://github.com/user-attachments/assets/6523dd6f-ef9a-4ae1-bddc-da716ea628cd" />
      <br/>
      <b>메인 화면</b>
    </td>
    <td align="center">
      <img width="640" height="360" alt="튜토리얼 화면" src="https://github.com/user-attachments/assets/969c5c16-1a5b-4dc4-83b7-46dbf460a651" />
      <br/>
      <b>튜토리얼 화면</b>
    </td>
  </tr>
  <tr>
    <td align="center">
      <img width="640" height="360" alt="전투 화면" src="https://github.com/user-attachments/assets/9d7d9063-ad9f-4a30-b522-277311fab6bd" />
      <br/>
      <b>전투 화면</b>
    </td>
    <td align="center">
      <img width="640" height="360" alt="점수 화면" src="https://github.com/user-attachments/assets/5c46caee-c5af-4f01-bfd3-c78a2369a775" />
      <br/>
      <b>점수 화면</b>
    </td>
  </tr>
</table>

---

## 🏗️ 프로젝트 구조

```
Assets/Scripts/
├── 📁 Editor/              # Unity Editor 확장 스크립트
│   └── StringDataDrawer.cs
│
├── 📁 Effect/              # 이펙트 및 피드백 시스템
│   ├── Effect.cs
│   ├── FeedbackEvent.cs
│   └── TailFollower.cs
│
├── 📁 Game/                # 게임플레이 핵심 로직
│   ├── Bullet.cs           # 총알 관리
│   ├── Monster.cs          # 적 캐릭터 관리
│   ├── PlayerController.cs # 플레이어 제어
│   ├── RedLine.cs          # 판정 라인
│   └── RhythmNode.cs       # 리듬 노트 관리
│
├── 📁 Manager/             # 게임 시스템 매니저
│   ├── AudioManager.cs     # 오디오 관리
│   ├── AudioSyncManager.cs # 음악 동기화
│   ├── GameManager.cs      # 게임 전체 관리
│   ├── InputManager.cs     # 입력 처리
│   ├── LoadingSceneManager.cs # 로딩 화면
│   ├── PatternGenerator.cs # 패턴 생성
│   └── SaveManager.cs      # 데이터 저장
│
├── 📁 ScriptableObject/    # 데이터 에셋
│   ├── Level Data/
│   │   └── LevelData.cs    # 레벨 데이터 정의
│   └── Monster Datas/
│       └── MonsterDatas.cs # 몬스터 데이터
│
├── 📁 UI/                  # UI 관련 스크립트
│   ├── UI_ConfirmPopup.cs  # 확인 팝업
│   ├── UI_GameEnd.cs       # 게임 종료 화면
│   ├── UI_QuitGame.cs      # 게임 종료
│   ├── UI_Title.cs         # 타이틀 화면
│   ├── UI_TotalRanking.cs  # 랭킹 화면
│   └── UI_Tutorial.cs      # 튜토리얼 화면
│
└── 📁 Utility/             # 유틸리티 클래스
    ├── CursorController.cs # 커서 제어
    ├── HoverImage.cs       # 이미지 호버 효과
    └── RankingNode.cs      # 랭킹 노드 관리
```

---

## 🔗 관련 링크
<table>
  <tr><td>시연 영상</td><td><a href="https://www.youtube.com/watch?v=NJPbLlls3Vc&feature=youtu.be">바로가기</a></td></tr>
  <tr><td>게임 상점</td><td><a href="https://store.onstove.com/ko/games/102473">바로가기</a></td></tr>
</table>

---

## 🛠 기술 스택

### 개발 환경
- **Engine** : Unity 6000.0.44f1 LTS
- **Language** : C#
- **IDE** : JetBrains Rider 2024.3.4f

### 주요 기술
- **Unity Audio** : 오디오 시스템 및 동기화
- **ScriptableObject** : 데이터 관리 및 설정
- **JsonUtility** : 로컬 데이터 저장

---

## 🏗 아키텍처

### 시스템 구성

```
┌─────────────────────────────────────────────────────────┐
│                    Game Manager                         │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐   │
│  │   Audio     │  │    Input     │  │     Save      │   │
│  │   Manager   │  │   Manager    │  │    Manager    │   │
│  └─────────────┘  └──────────────┘  └───────────────┘   │
└─────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────┐
│                AudioSyncManager                         │
│           (음악 BPM 동기화 및 패턴 생성)                  │
└─────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────┐
│                 Pattern Generator                       │
│            (리듬 패턴 및 노트 생성)                       │
└─────────────────────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────┐
│     RhythmNode ◄──► PlayerController ◄──► Monster       │
│   (노트 판정 로직)    (플레이어 제어)     (적 AI)         │
└─────────────────────────────────────────────────────────┘
```

### 핵심 워크플로우

#### 1️⃣ 게임 초기화
```
GameManager 시작
    ↓
AudioManager 초기화
    ↓
InputManager 설정
    ↓
SaveManager 데이터 로드
```

#### 2️⃣ 리듬 게임 루프
```
AudioSyncManager가 BPM 분석
    ↓
PatternGenerator가 박자에 맞춰 노트 생성
    ↓
RhythmNode 이동 및 판정
    ↓
PlayerController 입력 처리
    ↓
타격 판정 및 점수 계산
    ↓
FeedbackEffect 시각/청각 피드백
```

#### 3️⃣ 데이터 흐름
```
ScriptableObject (레벨/몬스터 데이터)
    ↓
Runtime 게임 로직
    ↓
SaveManager (점수 저장)
    ↓
JSON 파일 (로컬 저장)
```

### 주요 디자인 패턴

- **Singleton Pattern** : 매니저 클래스들의 전역 접근
- **ScriptableObject Pattern** : 데이터 중심 설계

---

