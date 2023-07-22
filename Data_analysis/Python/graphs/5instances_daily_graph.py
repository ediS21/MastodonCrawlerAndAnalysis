import pandas as pd
import matplotlib.pyplot as plt
import json

# List of files
files = [('GeneralData\MSocial-87k-users.json', 'blue'), 
         ('GeneralData\MCloud-125k-data.json', 'yellow'), 
         ('GeneralData\MstdnSocial-30k-data.json', 'green'), 
         ('GeneralData\MOnline-23k-data.json', 'red'), 
         ('GeneralData\MWorld-21k-data.json', 'orange')]

plt.figure(figsize=(16, 9))

for file, color in files:
    # Load the JSON data from the file
    data = []
    counter = 0
    with open(file, encoding='utf-8') as f:
        for line in f:
            counter += 1
            print(counter)
            json_data = json.loads(line)
            if json_data and 'created_at' in json_data:
                data.append(json_data)
    
    # Convert to a DataFrame
    df = pd.DataFrame(data)
    
    # Convert 'created_at' to datetime
    df['created_at'] = pd.to_datetime(df['created_at'])
    
    # Extract year, month and day
    df['year'] = df['created_at'].dt.year
    df['month'] = df['created_at'].dt.month
    df['day'] = df['created_at'].dt.day

    # Make a new column for year-month-day
    df['date'] = pd.to_datetime(df[['year', 'month', 'day']])
    
    # Count occurrences
    day_counts = df['date'].value_counts().sort_index()
    
    # Graph for November 2022
    day_counts_november = day_counts[(day_counts.index >= '2022-11-01') & (day_counts.index <= '2022-11-30')]
    plt.plot(day_counts_november.index.strftime('%Y-%m-%d'), day_counts_november.values, marker='o', color=color)

plt.title('Accounts Created Daily in November 2022')
plt.xlabel('Day')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.legend(['mastodon.social', 'mastodon.cloud', 'mstdn.social', 'mastodon.online', 'mastodon.world'], bbox_to_anchor=(1.05, 1), loc='upper left')
plt.xticks(rotation=45)
plt.tight_layout()
plt.show()
