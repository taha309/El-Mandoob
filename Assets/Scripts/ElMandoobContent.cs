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
/// a business, customer, neighborhood, order, payout and optional story beat.
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
        "ندى"
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

    private static readonly string[] Items =
    {
        "طلب أكل",
        "أدوية",
        "بقالة",
        "مستندات",
        "كتب وملازم",
        "عصير ومياه",
        "مخبوزات",
        "طرد صغير"
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
        ElMandoobOrder story = TryCreateStoryOrder(data);
        if (story != null)
        {
            return story;
        }

        int difficultyBonus = Mathf.Max(0, level - 1) * 4;
        int basePay = UnityEngine.Random.Range(32, 48) + difficultyBonus;
        int tip = UnityEngine.Random.Range(0, 4) == 0 ? UnityEngine.Random.Range(5, 16) : 0;

        string business = GetShiftBusiness(level);
        string customer = Pick(Customers);
        string area = GetShiftArea(level);
        string address = Pick(Addresses);
        string item = Pick(Items);

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

    private static ElMandoobOrder TryCreateStoryOrder(GameData data)
    {
        // Story jobs unlock gradually so normal deliveries and recurring customers
        // have time to establish the neighborhood first.
        if (data.storyStage == 0 && data.completedDeliveries >= 2)
        {
            return Story(
                "story_hassan_medicine", 0,
                "صيدلية النور", "عم حسن", "الدقي", "عمارة ١٢، الدور التالت",
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
                "مطبعة النيل", "شريف", "المهندسين", "العمارة اللي قصاد الصيدلية",
                "ظرف مقفول - مستندات",
                "في ظرف باسم شريف. قال بالحرف: يتسلّم زي ما هو.",
                "أنا مش اللي هستلم. طلّعه لندى في العنوان المكتوب ومتكلمهاش غير لما توصل.",
                "ندى استلمت الظرف، بس شكلها اتفاجئ لما شافت اسم المرسل.",
                70, 0, 2);
        }

        if (data.storyStage == 2 && data.completedDeliveries >= 8)
        {
            return Story(
                "story_wrong_order", 2,
                "بقالة عم سيد", "ندى", "العجوزة", "الدور التاني، الشقة اللي على الشمال",
                "كيس صغير + ظرف",
                "ندى طلبت حاجات بسيطة، بس في ظرف اتحط مع الطلب باسمها.",
                "أنا مطلبتش أي ظرف. استنى... الاسم اللي عليه اسمي فعلًا. سيبهولي.",
                "قبل ما تمشي، ندى قالتلك: لو شريف بعتلك حاجة تانية، كلمني الأول.",
                65, 8, 2);
        }

        if (data.storyStage == 3 && data.completedDeliveries >= 11)
        {
            return Story(
                "story_changed_address", 3,
                "مطبعة النيل", "شريف", "بين السرايات", "آخر الشارع، باب حديد أزرق",
                "طرد مستندات",
                "شريف سايب طرد وقال إن العنوان مهم جدًا.",
                "متروحش العنوان القديم. العنوان اتغير. آخر الشارع عند الباب الأزرق.",
                "محدش فتح الباب، لكن واحد نزل أخد الطرد من غير ما يقول اسمه.",
                85, 0, 3);
        }

        if (data.storyStage == 4 && data.completedDeliveries >= 14)
        {
            return Story(
                "story_hamdy_warning", 4,
                "كشري أبو حمدي", "عم حمدي", "المنيل", "جنب القهوة، العمارة القديمة",
                "طلب كشري + رسالة",
                "عم حمدي وقفك قبل ما تمشي وقالك إنه عايز يكلمك بعيد عن الزباين.",
                "بص يا ابني... الراجل اللي اسمه شريف ده بيسأل عن المندوبين أكتر ما بيسأل عن طلباته. خلي عينك مفتوحة.",
                "عم حمدي كتبلك رقم ندى وقالك: لو الموضوع زنق، كلمها.",
                55, 12, 3);
        }

        if (data.storyStage == 5 && data.completedDeliveries >= 17)
        {
            return Story(
                "story_final_envelope", 5,
                "مطبعة النيل", "ندى", "الدقي", "أول شارع على اليمين بعد الكشك",
                "آخر ظرف",
                "لقيت ظرف متساب باسمك أنت. جواه ورقة مكتوب عليها: وصّله لندى وبس.",
                "الورق ده يثبت إن في حد بيعمل طلبات وهمية ويرجع فلوسها على حسابات مختلفة. شريف كان بيستخدم المندوبين عشان محدش يربط العناوين ببعض.",
                "ندى خدت المستندات وقالت إنها هتتصرف فيها. أنت كملت شغلك، بس من النهارده بقيت تبص لكل طلب مرتين.",
                120, 20, 5);
        }

        return null;
    }

    public static void ApplyCompletedOrder(GameData data, ElMandoobOrder order)
    {
        if (order == null)
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
        if (storyStage <= 5) return "العناوين مش راكبة";
        return "آخر توصيل";
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
        return values[UnityEngine.Random.Range(0, values.Count)];
    }
}
