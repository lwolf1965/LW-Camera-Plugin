using System;
using System.Collections.Generic;

namespace ClientPlugin.Policy;

/// <summary>
/// Settings screen texts in the 13 languages the game ships besides English. Pure logic, no game types
/// (compiled into tests/). The English text is the key; a text without an entry is shown in English.
/// Column order = <see cref="Languages"/>.
/// </summary>
public static class Translations
{
    public static readonly string[] Languages =
        { "cs", "de", "es", "fr", "it", "ja", "ko", "pl", "pt-BR", "ru", "tr", "zh-Hans", "zh-Hant" };

    /// <summary>Our language for a game culture id ("de-DE", "zh-TW", "pt-BR"), or null for English.</summary>
    public static string LanguageFor(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture)) return null;
        var c = culture.Trim().ToLowerInvariant().Replace('_', '-');
        if (c.StartsWith("zh"))
            return c.Contains("hant") || c.Contains("tw") || c.Contains("hk") || c.Contains("mo") ? "zh-Hant" : "zh-Hans";
        if (c.StartsWith("pt")) return "pt-BR";
        var two = c.Length >= 2 ? c.Substring(0, 2) : c;
        return Array.IndexOf(Languages, two) >= 0 ? two : null;
    }

    /// <summary>The text in <paramref name="language"/> (from <see cref="LanguageFor"/>); English when missing.</summary>
    public static string Get(string english, string language)
    {
        if (string.IsNullOrEmpty(english) || language == null) return english;
        var column = Array.IndexOf(Languages, language);
        if (column < 0 || !Table.TryGetValue(english, out var row) || string.IsNullOrEmpty(row[column])) return english;
        return row[column];
    }

    // cs, de, es, fr, it, ja, ko, pl, pt-BR, ru, tr, zh-Hans, zh-Hant
    internal static readonly Dictionary<string, string[]> Table = new()
    {
        ["General"] = new[]
        {
            "Obecné", "Allgemein", "General", "Général", "Generale", "全般", "일반", "Ogólne", "Geral", "Общие", "Genel",
            "常规", "一般",
        },
        ["Keys"] = new[]
        {
            "Klávesy", "Tasten", "Teclas", "Touches", "Tasti", "キー", "키", "Klawisze", "Teclas", "Клавиши", "Tuşlar",
            "按键", "按鍵",
        },
        ["Zoom"] = new[]
        {
            "Přiblížení", "Zoom", "Zoom", "Zoom", "Zoom", "ズーム", "확대/축소", "Przybliżenie", "Zoom", "Приближение",
            "Yakınlaştırma", "缩放", "縮放",
        },
        ["Turret camera"] = new[]
        {
            "Kamera věže", "Geschützturm-Kamera", "Cámara de torreta", "Caméra de tourelle", "Telecamera torretta",
            "タレットカメラ", "포탑 카메라", "Kamera wieżyczki", "Câmera da torre", "Камера турели", "Taret kamerası",
            "炮塔摄像机", "砲塔攝影機",
        },
        ["Dock cameras"] = new[]
        {
            "Dokovací kamery", "Andockkameras", "Cámaras de atraque", "Caméras d'amarrage", "Telecamere di attracco",
            "ドッキングカメラ", "도킹 카메라", "Kamery dokowania", "Câmeras de acoplagem", "Камеры стыковки",
            "Kenetlenme kameraları", "对接摄像机", "對接攝影機",
        },
        ["Enabled"] = new[]
        {
            "Zapnuto", "Aktiviert", "Activado", "Activé", "Attivo", "有効", "사용", "Włączony", "Ativado", "Включён",
            "Etkin", "启用", "啟用",
        },
        ["Turn the whole plugin on or off"] = new[]
        {
            "Zapne nebo vypne celý plugin", "Schaltet das ganze Plugin ein oder aus", "Activa o desactiva todo el plugin",
            "Active ou désactive tout le plugin", "Attiva o disattiva l'intero plugin", "プラグイン全体をオン/オフにします",
            "플러그인 전체를 켜거나 끕니다", "Włącza lub wyłącza cały plugin", "Liga ou desliga o plugin inteiro",
            "Включает или выключает весь плагин", "Eklentinin tamamını açar veya kapatır", "开启或关闭整个插件",
            "開啟或關閉整個外掛",
        },
        ["Verbose log"] = new[]
        {
            "Podrobný log", "Ausführliches Log", "Registro detallado", "Journal détaillé", "Log dettagliato",
            "詳細ログ", "상세 로그", "Szczegółowy log", "Log detalhado", "Подробный лог", "Ayrıntılı günlük",
            "详细日志", "詳細記錄",
        },
        ["Write the camera list and camera switching to the game log (for testing)"] = new[]
        {
            "Zapisuje seznam kamer a přepínání kamer do logu hry (pro testování)",
            "Schreibt die Kameraliste und Kamerawechsel ins Spiel-Log (zum Testen)",
            "Escribe la lista de cámaras y los cambios de cámara en el registro del juego (para pruebas)",
            "Écrit la liste des caméras et les changements de caméra dans le journal du jeu (pour les tests)",
            "Scrive l'elenco delle telecamere e i cambi di telecamera nel log del gioco (per i test)",
            "カメラ一覧とカメラの切り替えをゲームのログに書き込みます（テスト用）",
            "카메라 목록과 카메라 전환을 게임 로그에 기록합니다(테스트용)",
            "Zapisuje listę kamer i przełączanie kamer do logu gry (do testów)",
            "Grava a lista de câmeras e a troca de câmeras no log do jogo (para testes)",
            "Записывает список камер и переключения камер в лог игры (для тестов)",
            "Kamera listesini ve kamera geçişlerini oyun günlüğüne yazar (test için)",
            "将摄像机列表和摄像机切换写入游戏日志（用于测试）",
            "將攝影機清單和攝影機切換寫入遊戲記錄（用於測試）",
        },
        ["Camera view on/off"] = new[]
        {
            "Pohled kamery zap/vyp", "Kameraansicht ein/aus", "Vista de cámara sí/no", "Vue caméra on/off",
            "Vista telecamera on/off", "カメラ視点 オン/オフ", "카메라 시점 켜기/끄기", "Widok kamery wł./wył.",
            "Visão da câmera liga/desliga", "Вид с камеры вкл/выкл", "Kamera görünümü aç/kapat", "摄像机视角 开/关",
            "攝影機視角 開/關",
        },
        ["In a cockpit: look out of a connector of this ship. Press again to go back"] = new[]
        {
            "V kokpitu: dívej se z konektoru této lodi. Dalším stiskem zpět",
            "Im Cockpit: aus einem Verbinder dieses Schiffs schauen. Erneut drücken, um zurückzukehren",
            "En la cabina: mira desde un conector de esta nave. Pulsa de nuevo para volver",
            "Dans le cockpit : regarder depuis un connecteur de ce vaisseau. Appuyer à nouveau pour revenir",
            "Nella cabina: guarda da un connettore di questa nave. Premi di nuovo per tornare",
            "コックピット内：この船のコネクターから外を見ます。もう一度押すと戻ります",
            "조종석에서: 이 함선의 커넥터에서 밖을 봅니다. 다시 누르면 돌아갑니다",
            "W kokpicie: patrz z konektora tego statku. Naciśnij ponownie, aby wrócić",
            "No cockpit: olhe a partir de um conector desta nave. Pressione de novo para voltar",
            "В кабине: смотреть из коннектора этого корабля. Нажмите ещё раз, чтобы вернуться",
            "Kokpitte: bu geminin bir bağlayıcısından bakın. Geri dönmek için tekrar basın",
            "在驾驶舱中：从本飞船的连接器向外看。再按一次返回",
            "在駕駛艙中：從本飛船的連接器向外看。再按一次返回",
        },
        ["Next connector"] = new[]
        {
            "Další konektor", "Nächster Verbinder", "Siguiente conector", "Connecteur suivant", "Connettore successivo",
            "次のコネクター", "다음 커넥터", "Następny konektor", "Próximo conector", "Следующий коннектор",
            "Sonraki bağlayıcı", "下一个连接器", "下一個連接器",
        },
        ["Previous connector"] = new[]
        {
            "Předchozí konektor", "Vorheriger Verbinder", "Conector anterior", "Connecteur précédent",
            "Connettore precedente", "前のコネクター", "이전 커넥터", "Poprzedni konektor", "Conector anterior",
            "Предыдущий коннектор", "Önceki bağlayıcı", "上一个连接器", "上一個連接器",
        },
        ["Mouse wheel zoom"] = new[]
        {
            "Přiblížení kolečkem myši", "Zoom mit dem Mausrad", "Zoom con la rueda del ratón", "Zoom à la molette",
            "Zoom con la rotella", "マウスホイールでズーム", "마우스 휠 확대/축소", "Przybliżanie kółkiem myszy",
            "Zoom com a roda do mouse", "Приближение колесом мыши", "Fare tekerleğiyle yakınlaştırma", "鼠标滚轮缩放",
            "滑鼠滾輪縮放",
        },
        ["Mouse wheel zooms the dock camera and the turret camera"] = new[]
        {
            "Kolečko myši přibližuje dokovací kameru i kameru věže",
            "Das Mausrad zoomt die Andockkamera und die Geschützturm-Kamera",
            "La rueda del ratón acerca la cámara de atraque y la de torreta",
            "La molette zoome la caméra d'amarrage et la caméra de tourelle",
            "La rotella ingrandisce la telecamera di attracco e quella della torretta",
            "マウスホイールでドッキングカメラとタレットカメラをズームします",
            "마우스 휠로 도킹 카메라와 포탑 카메라를 확대/축소합니다",
            "Kółko myszy przybliża kamerę dokowania i kamerę wieżyczki",
            "A roda do mouse aproxima a câmera de acoplagem e a câmera da torre",
            "Колесо мыши приближает камеру стыковки и камеру турели",
            "Fare tekerleği kenetlenme kamerasını ve taret kamerasını yakınlaştırır",
            "鼠标滚轮缩放对接摄像机和炮塔摄像机",
            "滑鼠滾輪縮放對接攝影機和砲塔攝影機",
        },
        ["Wide view (FOV °)"] = new[]
        {
            "Široký pohled (FOV °)", "Weitwinkel (FOV °)", "Vista amplia (FOV °)", "Vue large (FOV °)",
            "Vista ampia (FOV °)", "広角（FOV °）", "넓은 시야 (FOV °)", "Szeroki widok (FOV °)", "Visão ampla (FOV °)",
            "Широкий обзор (FOV °)", "Geniş görüş (FOV °)", "广角（FOV °）", "廣角（FOV °）",
        },
        ["Field of view when the dock camera is switched on"] = new[]
        {
            "Zorné pole po zapnutí dokovací kamery", "Sichtfeld beim Einschalten der Andockkamera",
            "Campo de visión al activar la cámara de atraque",
            "Champ de vision à l'activation de la caméra d'amarrage",
            "Campo visivo all'accensione della telecamera di attracco", "ドッキングカメラをオンにしたときの視野角",
            "도킹 카메라를 켰을 때의 시야각", "Pole widzenia po włączeniu kamery dokowania",
            "Campo de visão ao ligar a câmera de acoplagem", "Угол обзора при включении камеры стыковки",
            "Kenetlenme kamerası açıldığında görüş alanı", "开启对接摄像机时的视野", "開啟對接攝影機時的視野",
        },
        ["Barrel view"] = new[]
        {
            "Pohled z hlavně", "Laufansicht", "Vista desde el cañón", "Vue depuis le canon", "Vista dalla canna",
            "砲身視点", "포신 시점", "Widok z lufy", "Visão do cano", "Вид от ствола", "Namlu görünümü", "炮管视角",
            "砲管視角",
        },
        ["When you take control of a turret, the view moves to the turret's barrel. Off: the game's own turret view. The wheel zooms in both"] = new[]
        {
            "Když převezmeš ovládání věže, pohled se přesune na její hlaveň. Vyp: vlastní pohled hry. Kolečko přibližuje v obou",
            "Wenn du einen Geschützturm steuerst, wechselt die Ansicht zu seinem Lauf. Aus: die eigene Turmansicht des Spiels. Das Mausrad zoomt in beiden",
            "Al tomar el control de una torreta, la vista pasa a su cañón. Desactivado: la vista de torreta del juego. La rueda hace zoom en ambas",
            "Quand vous prenez le contrôle d'une tourelle, la vue passe sur son canon. Désactivé : la vue de tourelle du jeu. La molette zoome dans les deux",
            "Quando prendi il controllo di una torretta, la vista passa alla sua canna. Disattivato: la vista della torretta del gioco. La rotella fa zoom in entrambe",
            "タレットを操作すると、視点がタレットの砲身に移ります。オフ：ゲーム本来のタレット視点。どちらでもホイールでズームできます",
            "포탑을 조종하면 시점이 포탑의 포신으로 이동합니다. 끔: 게임 기본 포탑 시점. 두 경우 모두 휠로 확대/축소됩니다",
            "Gdy przejmiesz sterowanie wieżyczką, widok przechodzi na jej lufę. Wył.: własny widok wieżyczki z gry. Kółko przybliża w obu",
            "Ao assumir o controle de uma torre, a visão vai para o cano dela. Desligado: a visão de torre do próprio jogo. A roda dá zoom nas duas",
            "Когда вы берёте турель под управление, вид переходит на её ствол. Выкл.: собственный вид турели в игре. Колесо приближает в обоих",
            "Bir taretin kontrolünü aldığınızda görünüm taretin namlusuna geçer. Kapalı: oyunun kendi taret görünümü. Tekerlek ikisinde de yakınlaştırır",
            "控制炮塔时，视角移到炮塔的炮管上。关闭：使用游戏自带的炮塔视角。两种视角都可用滚轮缩放",
            "控制砲塔時，視角移到砲塔的砲管上。關閉：使用遊戲內建的砲塔視角。兩種視角都可用滾輪縮放",
        },
        ["All turrets"] = new[]
        {
            "Všechny věže", "Alle Geschütztürme", "Todas las torretas", "Toutes les tourelles", "Tutte le torrette",
            "すべてのタレット", "모든 포탑", "Wszystkie wieżyczki", "Todas as torres", "Все турели", "Tüm taretler",
            "所有炮塔", "所有砲塔",
        },
        ["On: every turret gets the barrel view. Off: only turrets with [CAM] in the name"] = new[]
        {
            "Zap: každá věž má pohled z hlavně. Vyp: jen věže s [CAM] v názvu",
            "Ein: jeder Geschützturm bekommt die Laufansicht. Aus: nur Türme mit [CAM] im Namen",
            "Activado: todas las torretas tienen la vista desde el cañón. Desactivado: solo las torretas con [CAM] en el nombre",
            "Activé : chaque tourelle a la vue depuis le canon. Désactivé : seulement les tourelles avec [CAM] dans le nom",
            "Attivo: ogni torretta ha la vista dalla canna. Disattivo: solo le torrette con [CAM] nel nome",
            "オン：すべてのタレットが砲身視点になります。オフ：名前に [CAM] があるタレットだけ",
            "켬: 모든 포탑이 포신 시점을 사용합니다. 끔: 이름에 [CAM]이 있는 포탑만",
            "Wł.: każda wieżyczka ma widok z lufy. Wył.: tylko wieżyczki z [CAM] w nazwie",
            "Ligado: toda torre tem a visão do cano. Desligado: só torres com [CAM] no nome",
            "Вкл.: вид от ствола у каждой турели. Выкл.: только у турелей с [CAM] в названии",
            "Açık: her taret namlu görünümünü kullanır. Kapalı: yalnızca adında [CAM] olan taretler",
            "开启：所有炮塔都使用炮管视角。关闭：仅名称中带 [CAM] 的炮塔",
            "開啟：所有砲塔都使用砲管視角。關閉：僅名稱中帶 [CAM] 的砲塔",
        },
        ["All connectors are cameras"] = new[]
        {
            "Všechny konektory jsou kamery", "Alle Verbinder sind Kameras", "Todos los conectores son cámaras",
            "Tous les connecteurs sont des caméras", "Tutti i connettori sono telecamere", "すべてのコネクターをカメラにする",
            "모든 커넥터를 카메라로 사용", "Wszystkie konektory są kamerami", "Todos os conectores são câmeras",
            "Все коннекторы — камеры", "Tüm bağlayıcılar kameradır", "所有连接器都是摄像机", "所有連接器都是攝影機",
        },
        ["On: every connector of the ship is a dock camera, no tag needed. Off: only connectors with [CAM] in the name"] = new[]
        {
            "Zap: každý konektor lodi je dokovací kamera, bez štítku. Vyp: jen konektory s [CAM] v názvu",
            "Ein: jeder Verbinder des Schiffs ist eine Andockkamera, kein Tag nötig. Aus: nur Verbinder mit [CAM] im Namen",
            "Activado: cada conector de la nave es una cámara de atraque, sin etiqueta. Desactivado: solo los conectores con [CAM] en el nombre",
            "Activé : chaque connecteur du vaisseau est une caméra d'amarrage, sans tag. Désactivé : seulement les connecteurs avec [CAM] dans le nom",
            "Attivo: ogni connettore della nave è una telecamera di attracco, senza tag. Disattivo: solo i connettori con [CAM] nel nome",
            "オン：船のすべてのコネクターがドッキングカメラになり、タグは不要です。オフ：名前に [CAM] があるコネクターだけ",
            "켬: 함선의 모든 커넥터가 도킹 카메라가 되며 태그가 필요 없습니다. 끔: 이름에 [CAM]이 있는 커넥터만",
            "Wł.: każdy konektor statku jest kamerą dokowania, bez tagu. Wył.: tylko konektory z [CAM] w nazwie",
            "Ligado: todo conector da nave é uma câmera de acoplagem, sem tag. Desligado: só conectores com [CAM] no nome",
            "Вкл.: каждый коннектор корабля — камера стыковки, тег не нужен. Выкл.: только коннекторы с [CAM] в названии",
            "Açık: geminin her bağlayıcısı bir kenetlenme kamerasıdır, etiket gerekmez. Kapalı: yalnızca adında [CAM] olan bağlayıcılar",
            "开启：飞船的每个连接器都是对接摄像机，无需标签。关闭：仅名称中带 [CAM] 的连接器",
            "開啟：飛船的每個連接器都是對接攝影機，無需標籤。關閉：僅名稱中帶 [CAM] 的連接器",
        },
        ["Camera offset (m)"] = new[]
        {
            "Odsazení kamery (m)", "Kamera-Abstand (m)", "Desplazamiento de la cámara (m)", "Décalage de la caméra (m)",
            "Distanza della telecamera (m)", "カメラの距離（m）", "카메라 거리 (m)", "Odsunięcie kamery (m)",
            "Distância da câmera (m)", "Смещение камеры (м)", "Kamera uzaklığı (m)", "摄像机偏移（米）", "攝影機偏移（公尺）",
        },
        ["How far in front of the connector's face the camera sits. A number in the tag overrides it: [CAM 2]"] = new[]
        {
            "Jak daleko před čelem konektoru kamera je. Číslo ve štítku to přepíše: [CAM 2]",
            "Wie weit vor der Verbinderfläche die Kamera sitzt. Eine Zahl im Tag überschreibt das: [CAM 2]",
            "A qué distancia delante de la cara del conector está la cámara. Un número en la etiqueta lo cambia: [CAM 2]",
            "Distance de la caméra devant la face du connecteur. Un nombre dans le tag la remplace : [CAM 2]",
            "Quanto davanti alla faccia del connettore si trova la telecamera. Un numero nel tag la sostituisce: [CAM 2]",
            "コネクターの接続面からカメラまでの距離。タグ内の数値が優先されます：[CAM 2]",
            "커넥터 면 앞에 카메라가 놓이는 거리입니다. 태그의 숫자가 우선합니다: [CAM 2]",
            "Jak daleko przed czołem konektora jest kamera. Liczba w tagu to zmienia: [CAM 2]",
            "A que distância à frente da face do conector fica a câmera. Um número na tag substitui: [CAM 2]",
            "Насколько впереди торца коннектора стоит камера. Число в теге заменяет его: [CAM 2]",
            "Kameranın bağlayıcı yüzünün ne kadar önünde durduğu. Etiketteki sayı bunu geçersiz kılar: [CAM 2]",
            "摄像机位于连接器端面前方的距离。标签中的数字优先：[CAM 2]",
            "攝影機位於連接器端面前方的距離。標籤中的數字優先：[CAM 2]",
        },
        ["Controls follow dock camera"] = new[]
        {
            "Ovládání podle dokovací kamery", "Steuerung folgt der Andockkamera", "Los controles siguen la cámara de atraque",
            "Les commandes suivent la caméra d'amarrage", "I comandi seguono la telecamera di attracco",
            "操作をドッキングカメラに合わせる", "조작이 도킹 카메라를 따름", "Sterowanie według kamery dokowania",
            "Controles seguem a câmera de acoplagem", "Управление по камере стыковки",
            "Kontroller kenetlenme kamerasını izler", "操控跟随对接摄像机", "操控跟隨對接攝影機",
        },
        ["On: in the dock camera W flies where the camera looks, A D C Space move left, right, down, up on the screen, the mouse turns around the camera. Off: controls stay as in the cockpit"] = new[]
        {
            "Zap: v dokovací kameře W letí tam, kam se kamera dívá, A D C mezerník posouvají doleva, doprava, dolů, nahoru na obrazovce, myš otáčí kolem kamery. Vyp: ovládání zůstává jako v kokpitu",
            "Ein: in der Andockkamera fliegt W dorthin, wohin die Kamera schaut, A D C Leertaste bewegen nach links, rechts, unten, oben auf dem Bildschirm, die Maus dreht um die Kamera. Aus: Steuerung wie im Cockpit",
            "Activado: en la cámara de atraque, W vuela hacia donde mira la cámara, A D C Espacio mueven a la izquierda, derecha, abajo y arriba en la pantalla, el ratón gira alrededor de la cámara. Desactivado: controles como en la cabina",
            "Activé : dans la caméra d'amarrage, W vole là où regarde la caméra, A D C Espace déplacent à gauche, à droite, en bas, en haut à l'écran, la souris tourne autour de la caméra. Désactivé : commandes comme dans le cockpit",
            "Attivo: nella telecamera di attracco W vola dove guarda la telecamera, A D C Spazio muovono a sinistra, destra, giù, su sullo schermo, il mouse ruota attorno alla telecamera. Disattivo: comandi come nella cabina",
            "オン：ドッキングカメラ中、W でカメラの向く方へ飛び、A D C スペースで画面の左・右・下・上へ移動し、マウスでカメラを中心に回転します。オフ：コックピットと同じ操作",
            "켬: 도킹 카메라에서 W는 카메라가 보는 방향으로 날고, A D C 스페이스는 화면 기준 왼쪽, 오른쪽, 아래, 위로 이동하며, 마우스는 카메라를 기준으로 회전합니다. 끔: 조종석과 같은 조작",
            "Wł.: w kamerze dokowania W leci tam, gdzie patrzy kamera, A D C Spacja przesuwają w lewo, w prawo, w dół, w górę na ekranie, mysz obraca wokół kamery. Wył.: sterowanie jak w kokpicie",
            "Ligado: na câmera de acoplagem, W voa para onde a câmera olha, A D C Espaço movem para a esquerda, direita, baixo e cima na tela, o mouse gira em torno da câmera. Desligado: controles como no cockpit",
            "Вкл.: в камере стыковки W летит туда, куда смотрит камера, A D C Пробел двигают влево, вправо, вниз, вверх по экрану, мышь поворачивает вокруг камеры. Выкл.: управление как в кабине",
            "Açık: kenetlenme kamerasında W kameranın baktığı yöne uçar, A D C Boşluk ekranda sola, sağa, aşağı, yukarı hareket ettirir, fare kamera etrafında döndürür. Kapalı: kontroller kokpitteki gibi",
            "开启：在对接摄像机中，W 朝摄像机看的方向飞，A D C 空格键按屏幕向左、右、下、上移动，鼠标绕摄像机转动。关闭：操控与驾驶舱相同",
            "開啟：在對接攝影機中，W 朝攝影機看的方向飛，A D C 空白鍵按畫面向左、右、下、上移動，滑鼠繞攝影機轉動。關閉：操控與駕駛艙相同",
        },
        ["Click to bind a key (press Esc to cancel). Right-click to clear."] = new[]
        {
            "Klikni pro přiřazení klávesy (Esc zruší). Pravým kliknutím smažeš.",
            "Klicken, um eine Taste zuzuweisen (Esc bricht ab). Rechtsklick löscht.",
            "Haz clic para asignar una tecla (Esc cancela). Clic derecho para borrar.",
            "Cliquer pour assigner une touche (Échap pour annuler). Clic droit pour effacer.",
            "Clicca per assegnare un tasto (Esc per annullare). Clic destro per cancellare.",
            "クリックしてキーを割り当て（Escで取り消し）。右クリックで解除。",
            "클릭하여 키를 지정합니다(Esc로 취소). 오른쪽 클릭으로 지웁니다.",
            "Kliknij, aby przypisać klawisz (Esc anuluje). Prawy przycisk czyści.",
            "Clique para atribuir uma tecla (Esc cancela). Clique direito para limpar.",
            "Нажмите, чтобы назначить клавишу (Esc — отмена). Правый клик — очистить.",
            "Tuş atamak için tıklayın (Esc iptal eder). Temizlemek için sağ tıklayın.",
            "单击以绑定按键（按 Esc 取消）。右键单击清除。",
            "按一下以綁定按鍵（按 Esc 取消）。按右鍵清除。",
        },
        ["None"] = new[]
        {
            "Žádná", "Keine", "Ninguna", "Aucune", "Nessuno", "なし", "없음", "Brak", "Nenhuma", "Нет", "Yok", "无", "無",
        },
    };
}
