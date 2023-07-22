import pandas as pd
import matplotlib.pyplot as plt
import json

# Input file
file = 'GeneralData\MWorld-21k-data.json'

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

# Year by year graph (from 2021 to 2023)
year_counts = df['year'].value_counts().sort_index()
year_counts = year_counts[year_counts.index >= 2021]  
plt.figure(figsize=(10, 6))
plt.plot(year_counts.index, year_counts.values, marker='o')
plt.title('Accounts Created Per Year (2021 - 2023)')
plt.xlabel('Year')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.xticks(range(2021, 2024))  # Set the x-axis ticks to only include integer years
plt.show()

# Month by month graph (from May 2022 to May 2023)
df['month_year'] = df['created_at'].dt.to_period('M')
month_counts = df['month_year'].value_counts().sort_index()
month_counts_selected_year = month_counts[(month_counts.index >= '2022-05') & (month_counts.index <= '2023-05')]
plt.figure(figsize=(10, 6))
plt.plot(month_counts_selected_year.index.strftime('%Y-%m'), month_counts_selected_year.values, marker='o')
plt.title('Accounts Created Per Month (May 2022 - May 2023)')
plt.xlabel('Month')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.show()

# Day by day graph (from 1st November 2022 till 30th November 2022)
day_counts_november = day_counts[(day_counts.index >= '2022-11-01') & (day_counts.index <= '2022-11-30')]
plt.figure(figsize=(10, 6))
plt.plot(day_counts_november.index.strftime('%Y-%m-%d'), day_counts_november.values, marker='o')
plt.title('Accounts Created Per Day (1st November 2022 - 30th November 2022)')
plt.xlabel('Day')
plt.ylabel('Number of Accounts')
plt.grid(True)
plt.xticks(rotation=45)
plt.tight_layout()
plt.show()
