using Newtonsoft.Json.Linq;
using GameServer.Domain.Object.Components;


namespace GameServer.Infrastracture.Factory
{
    public static class ChoiceFactory
    {
        public static Choice FromJson(JObject json)
        {
            var choice = new Choice
            {
                Label = json["Label"]?.ToString(),
                RiskId = json["RiskId"]?.ToString(),
                OverrideActions = ParseOverrideActions(json["OverrideActions"])
            };

            return choice;
        }

        public static List<Choice> FromJsonArray(JArray array)
        {
            return array.Select(token => FromJson((JObject)token)).ToList();
        }

        private static List<ActionEntity> ParseOverrideActions(JToken token)
        {
            var list = new List<ActionEntity>();
            if (token is not JArray array) return list;

            foreach (var item in array)
            {
                var target = item["target"]?.ToString() switch
                {
                    "Self" => TargetType.Self,
                    "Held" => TargetType.HeldItem,
                    _ => TargetType.HeldItem,
                };

                list.Add(new ActionEntity()
                {
                    id = item["id"]?.ToString(),
                    label = item["label"]?.ToString(),
                    riskChange = item["riskChange"]?.ToObject<int>() ?? 0,
                    actionPointCost = item["ActionPointCost"]?.ToObject<int>() ?? 0,
                    target = target,
                    ObjectAttributes = item["ObjectAttributes"]?.ToObject<List<string>>() ?? new(),
                    Explanation = item["Explanation"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}