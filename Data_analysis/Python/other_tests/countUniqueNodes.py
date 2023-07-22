import json

def count_unique_numbers(json_file):
    unique_numbers = set()

    class CustomDecoder(json.JSONDecoder):
        def __init__(self, *args, **kwargs):
            kwargs['parse_float'] = lambda x: float(x)
            super().__init__(*args, **kwargs)

    with open(json_file, 'r', encoding='utf-8') as file:
        for line in file:
            line = line.strip()
            if line:
                json_obj = json.loads(line, cls=CustomDecoder)
                if isinstance(json_obj, list):
                    for item in json_obj:
                        if isinstance(item, dict):
                            for key, value in item.items():
                                if isinstance(value, (int, float)):
                                    unique_numbers.add(value)
                                elif isinstance(value, str):
                                    try:
                                        number = int(value)
                                        unique_numbers.add(number)
                                    except ValueError:
                                        try:
                                            number = float(value)
                                            unique_numbers.add(number)
                                        except ValueError:
                                            pass

    return len(unique_numbers)

# Usage example
file_name = 'test2.json'
unique_count = count_unique_numbers(file_name)
print("Number of unique numbers:", unique_count)
