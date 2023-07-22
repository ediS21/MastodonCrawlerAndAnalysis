import pandas as pd
import matplotlib.pyplot as plt
import json

# List of files
files = [('GeneralData\MSocial-87k-users.json', 'blue'), 
         ('GeneralData\MCloud-125k-data.json', 'yellow'), 
         ('GeneralData\MstdnSocial-30k-data.json', 'green'), 
         ('GeneralData\MOnline-23k-data.json', 'red'), 
         ('GeneralData\MWorld-21k-data.json', 'orange')]

plt.figure(figsize=(10, 6))

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
    
    # Extract year and month
    df['year'] = df['created_at'].dt.year
    df['month'] = df['created_at'].dt.to_period('M')
    
    # Count occurrences
    month_counts = df['month'].value_counts().sort_index()
    
    # Month by month graph (from August 2022 to May 2023)
    month_counts_last_year = month_counts[(month_counts.index >= '2022-01') & (month_counts.index <= '2023-05')]
    plt.plot(month_counts_last_year.index.strftime('%Y-%m'), month_counts_last_year.values, marker='o', color=color)
    
plt.title('Accounts Created Per Month (Janyary 2022 - May 2023)')
plt.xlabel('Month')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.legend(['mastodon.social', 'mastodon.cloud', 'mstdn.social', 'mastodon.online', 'mastodon.world'])
plt.xticks(rotation=45)
plt.show()
