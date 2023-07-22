import pandas as pd
import matplotlib.pyplot as plt
import json

# List of files
files = [('GeneralData/MSocial-87k-users.json', 'blue'), 
         ('GeneralData/MCloud-125k-data.json', 'yellow'), 
         ('GeneralData/MstdnSocial-30k-data.json', 'green'), 
         ('GeneralData/MOnline-23k-data.json', 'red'), 
         ('GeneralData/MWorld-21k-data.json', 'orange')]

plt.figure(figsize=(10, 6))
max_year = 0  # This will hold the maximum year from all data
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
    year_counts = df['year'].value_counts().sort_index()
    month_counts = df['month'].value_counts().sort_index()
    
    # Count occurrences for years 2016 onwards
    year_counts = df[df['year'] >= 2016]['year'].value_counts().sort_index()
    
    # Convert index to integers
    year_counts.index = year_counts.index.astype(int)
    
    # Find the maximum year in this data
    if max_year < year_counts.index.max():
        max_year = year_counts.index.max()

    # Year by year graph
    plt.plot(year_counts.index, year_counts.values, marker='o', color=color, alpha=0.7)
    
plt.title('Accounts Created Per Year')
plt.xlabel('Year')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.legend(['mastodon.social', 'mastodon.cloud', 'mstdn.social', 'mastodon.online', 'mastodon.world'])
plt.xticks(range(2016, max_year + 1))  # Set the x-axis range from 2016 to the max year
plt.show()
