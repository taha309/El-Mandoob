using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ElMandoobOrder
{
    public string id;
    public string businessName;
    public string customerName;
    public string areaName;
    public string address;
    public string itemDescription;
    public string pickupMessage;
    public string customerMessage;
    public string deliveredMessage;
    public int basePay;
    public int tip;
    public int reputationReward;
    public bool storyOrder;
    public int storyStage;

    public int TotalPay
    {
        get { return basePay + tip; }
    }
}

/// <summary>
/// Egyptian content database for El Mandoob.
/// Keeps the ready-made delivery loop intact while giving every generated job
/// a business, customer, neighborhood, sensible order, payout and optional story beat.
/// </summary>
public static class ElMandoobContent
{
    private static readonly string[] Businesses =
    {
        "كشري أبو حمدي",
        "صيدلية النور",
        "فرن الحارة",
        "بقالة عم سيد",
        "مطبعة النيل",
        "بيتزا المعلم",
        "عصير قصب السعادة",
        "مكتبة الطالب"
    };

    private static readonly string[] Customers =
    {
        "منة",
        "عم حسن",
        "يوسف",
        "نهى",
        "كريم",
        "سارة",
        "محمود",
        "أم آسر",
        "حازم",
        "عمر"
    };

    private static readonly string[] Areas =
    {
        "الدقي",
        "العجوزة",
        "المنيل",
        "المهندسين",
        "بين السرايات",
        "الجيزة",
        "إمبابة",
        "الهرم"
    };

    private static readonly string[] Addresses =
    {
        "عمارة ١٢، الدور التالت",
        "عمارة ٢٧، جنب السوبر ماركت",
        "الدور الخامس، الأسانسير بايظ",
        "أول شارع على اليمين بعد الكشك",
        "العمارة اللي قصاد الصيدلية",
        "الدور التاني، الشقة اللي على الشمال",
        "آخر الشارع، باب حديد أزرق",
        "جنب القهوة، العمارة القديمة"
    };

    private static readonly string[] PickupMessages =
    {
        "الطلب جاهز. متتأخرش على الزبون.",
        "خلي بالك من الطلب في الطريق.",
        "الزبون مستنيك، يلا بينا.",
        "استلم الطلب واتأكد من العنوان.",
        "الطلب ده مستعجل شوية."
    };

    private static readonly string[] CustomerMessages =
    {
        "كلمني لما توصل تحت.",
        "أنا مستنيك عند باب العمارة.",
        "لو ملقتش العمارة اسأل عند الكشك.",
        "خلي بالك، مفيش فكة كبيرة معايا.",
        "اطلع فوق لو سمحت، الأسانسير شغال النهارده.",
        "متقلقش لو اللوكيشن مزحلق شوية، العمارة بعدها على طول."
    };

    public static ElMandoobOrder CreateOrder(GameData data, int level)
    {
        ElMandoobOrder story = TryCreateStoryOrder(data, level);
        if (story != null)
        {
            return story;
        }

        int difficultyBonus = Mathf.Max(0, level - 1) * 4;
        int basePay = UnityEngine.Random.Range(32, 48) + difficultyBonus;
        int tip = UnityEngine.Random.Range(0, 4) == 0 ? UnityEngine.Random.Range(5, 16) : 0;

        string business = PickBusinessForLevel(level);
        string customer = Pick(Customers);
        string area = GetShiftArea(level);
        string address = Pick(Addresses);
        string item = PickItemForBusiness(business);

        return new ElMandoobOrder
        {
            id = "normal_" + Guid.NewGuid().ToString("N"),
            businessName = business,
            customerName = customer,
            areaName = area,
            address = address,
            itemDescription = item,
            pickupMessage = Pick(PickupMessages),
            customerMessage = Pick(CustomerMessages),
            deliveredMessage = tip > 0
                ? "تسلم يا باشا. خد " + tip + " جنيه زيادة عشان وصلت بسرعة."
                : "تسلم يا باشا، ربنا معاك.",
            basePay = basePay,
            tip = tip,
            reputationReward = 1,
            storyOrder = false,
            storyStage = -1
        };
    }

    public static string GetShiftArea(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, Areas.Length - 1);
        return Areas[index];
    }

    public static string GetShiftBusiness(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, Businesses.Length - 1);
        return Businesses[index];
    }

    private static string PickBusinessForLevel(int level)
    {
        // Early shifts expose a smaller neighborhood roster, then more businesses join in.
        int availableCount = Mathf.Clamp(3 + (Mathf.Max(1, level) - 1) / 2, 3, Businesses.Length);
        return Businesses[UnityEngine.Random.Range(0, availableCount)];
    }

    private static string PickItemForBusiness(string business)
    {
        switch (business)
        {
            case "كشري أبو حمدي":
                return Pick(new[] { "كشري كبير", "كشري وسط + دقة", "٢ كشري صغير", "كشري كبير + شطة" });
            case "صيدلية النور":
                return Pick(new[] { "أدوية وروشتة", "دواء ضغط", "فيتامينات", "طلب صيدلية صغير" });
            case "فرن الحارة":
                return Pick(new[] { "عيش ومخبوزات", "فينو وعيش", "فطير صغير", "مخبوزات سخنة" });
            case "بقالة عم سيد":
                return Pick(new[] { "بقالة للبيت", "مياه وعصير", "سكر وشاي", "منظفات وحاجات للبيت" });
            case "مطبعة النيل":
                return Pick(new[] { "مستندات مطبوعة", "ملف أوراق", "نسخ وتصوير", "ظرف مستندات" });
            case "بيتزا المعلم":
                return Pick(new[] { "بيتزا وسط", "٢ بيتزا صغيرة", "بيتزا كبيرة + مشروب", "بيتزا خضار" });
            case "عصير قصب السعادة":
                return Pick(new[] { "٢ قصب", "عصير مانجا", "كوكتيل فواكه", "قصب + مياه" });
            case "مكتبة الطالب":
                return Pick(new[] { "كتب وملازم", "ملازم مطبوعة", "أدوات مكتبية", "دفاتر وأقلام" });
            default:
                return "طرد صغير";
        }
    }

    private static ElMandoobOrder TryCreateStoryOrder(GameData data, int level)
    {
        if (data == null)
        {
            return null;
        }

        string shiftArea = GetShiftArea(level);

        // Story jobs unlock gradually so normal deliveries and recurring customers
        // have time to establish the neighborhood first.
        if (data.storyStage == 0 && data.completedDeliveries >= 2)
        {
            return Story(
                "story_hassan_medicine", 0,
                "صيدلية النور", "عم حسن", shiftArea, "عمارة ١٢، الدور التالت",
                "دواء ضغط وسكر",
                "الطلب ده لعم حسن. راجل كبير وبيطلب مننا كل أسبوع.",
                "يا ابني لو طلعتلي فوق تبقى جدع، رجلي واجعاني النهارده.",
                "ربنا يكرمك يا ابني. خليك فاكر الوشوش الكويسة في الشغلانة دي.",
                45, 10, 2);
        }

        if (data.storyStage == 1 && data.completedDeliveries >= 5)
        {
            return Story(
                "story_sealed_envelope", 1,
                "مطبعة النيل", "ندى", shiftArea, "العمارة اللي قصاد الصيدلية",
                "ظرف مقفول - مستندات",
                "شريف سايب ظرف لندى وقال بالحرف: يتسلّم زي ما هو.",
                "ندى: أنا مستنياك تحت. بصراحة أنا مش فاهمة شريف باعتلي إيه.",
                "ندى استلمت الظرف، وأول ما شافت اسم شريف ملامحها اتغيرت.",
                70, 0, 2);
        }

        if (data.storyStage == 2 && data.completedDeliveries >= 8)
        {
            return Story(
                "story_wrong_order", 2,
                "بقالة عم سيد", "ندى", shiftArea, "الدور التاني، الشقة اللي على الشمال",
                "كيس بقالة صغير + ظرف",
                "ندى طلبت حاجات بسيطة، بس في ظرف اتحط مع الطلب باسمها.",
                "ندى: أنا مطلبتش أي ظرف. استنى... الاسم اللي عليه اسمي فعلًا. سيبهولي.",
                "قبل ما تمشي، ندى قالتلك: لو شريف بعتلك حاجة تانية، حاول تعرف جاية منين.",
                65, 8, 2);
        }

        if (data.storyStage == 3 && data.completedDeliveries >= 11)
        {
            return Story(
                "story_changed_address", 3,
                "مطبعة النيل", "مستلم من طرف شريف", shiftArea, "آخر الشارع، باب حديد أزرق",
                "طرد مستندات",
                "شريف سايب طرد وقال إن العنوان مهم جدًا.",
                "رسالة من شريف: متروحش العنوان القديم. روح آخر الشارع عند الباب الأزرق.",
                "محدش فتح الباب. واحد نزل أخد الطرد بسرعة ومقالش حتى اسمه.",
                85, 0, 3);
        }

        if (data.storyStage == 4 && data.completedDeliveries >= 14)
        {
            return Story(
                "story_hamdy_warning", 4,
                "كشري أبو حمدي", "مريم", shiftArea, "جنب القهوة، العمارة القديمة",
                "كشري كبير",
                "وأنت بتستلم الطلب، عم حمدي وطّى صوته وقالك: الراجل اللي اسمه شريف بيسأل عن المندوبين أكتر ما بيسأل عن طلباته. خلي عينك مفتوحة.",
                "مريم: أنا جنب القهوة بالظبط، العمارة القديمة. متتوهش مني.",
                "وصلت الطلب، بس كلام عم حمدي فضّل في دماغك. قبل ما تمشي كان مديلك رقم ندى احتياطي.",
                55, 12, 3);
        }

        if (data.storyStage == 5 && data.completedDeliveries >= 17)
        {
            return Story(
                "story_final_envelope", 5,
                "مطبعة النيل", "ندى", shiftArea, "أول شارع على اليمين بعد الكشك",
                "آخر ظرف",
                "لقيت ظرف متساب باسمك أنت. جواه ورقة واحدة: وصّله لندى وبس.",
                "ندى: الورق ده مهم. هفهمك لما توصل.",
                "ندى شرحتلك إن الورق بيربط طلبات وهمية بفلوس بترجع على حسابات مختلفة. شريف كان بيستخدم المندوبين عشان العناوين تفضل منفصلة عن بعض.",
                120, 20, 5);
        }

        return null;
    }

    public static void ApplyCompletedOrder(GameData data, ElMandoobOrder order)
    {
        if (data == null || order == null)
        {
            return;
        }

        if (order.storyOrder && order.storyStage == data.storyStage)
        {
            data.storyStage++;
        }
    }

    public static string GetStoryChapterName(int storyStage)
    {
        if (storyStage <= 0) return "أول شيفت";
        if (storyStage == 1) return "الناس بقت تعرفك";
        if (storyStage <= 4) return "الطلب الغريب";
        if (storyStage == 5) return "العناوين مش راكبة";
        return "حكاية الحي خلصت";
    }

    public static string GetNextStoryHint(GameData data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        int target;
        switch (data.storyStage)
        {
            case 0: target = 2; break;
            case 1: target = 5; break;
            case 2: target = 8; break;
            case 3: target = 11; break;
            case 4: target = 14; break;
            case 5: target = 17; break;
            default: return "حكاية الحي الأولى خلصت.";
        }

        int remaining = Mathf.Max(0, target - data.completedDeliveries);
        if (remaining == 0)
        {
            return "في طلب خاص مستنيك في الشيفت الجاي.";
        }
        if (remaining == 1)
        {
            return "توصيلة واحدة كمان وطلب خاص هيفتح.";
        }
        if (remaining == 2)
        {
            return "توصيلتين كمان وطلب خاص هيفتح.";
        }

        return remaining + " توصيلات كمان وطلب خاص هيفتح.";
    }

    private static ElMandoobOrder Story(
        string id,
        int stage,
        string business,
        string customer,
        string area,
        string address,
        string item,
        string pickup,
        string customerMessage,
        string delivered,
        int basePay,
        int tip,
        int reputation)
    {
        return new ElMandoobOrder
        {
            id = id,
            businessName = business,
            customerName = customer,
            areaName = area,
            address = address,
            itemDescription = item,
            pickupMessage = pickup,
            customerMessage = customerMessage,
            deliveredMessage = delivered,
            basePay = basePay,
            tip = tip,
            reputationReward = reputation,
            storyOrder = true,
            storyStage = stage
        };
    }

    private static string Pick(IList<string> values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        return values[UnityEngine.Random.Range(0, values.Count)];
    }
}
