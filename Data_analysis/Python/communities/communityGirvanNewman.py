import networkx as nx
from networkx.algorithms import community
import json
import time

#NOT USED - GIRVAN-NEWMAN TOOK TOO LONG TO COMPUTE ON HABROK

graph = nx.Graph()

start_time = time.time()

counter = 0

# Open and read the JSON file
with open('test.json', 'r', encoding='utf-8') as file:
    # iterate over each line in the file
    for line in file:
        # remove newline characters and leading/trailing whitespaces
        line = line.strip()
        # skip if line is empty
        if not line:
            continue
        # Load the JSON object from the line
        json_obj = json.loads(line)
        # iterate over the list of dictionaries in json_obj
        for item in json_obj:
            source_id = str(item['source_id'])  # convert to string
            target_id = str(item['target_id'])  # convert to string
            profile_url = item['ProfileUrl']
            # Add edge only if both all items are not None and profile_url contains 'mastodon.social'
            if target_id and profile_url and 'mastodon.social' in profile_url:
                counter += 1
                print(counter)
                graph.add_edge(source_id, target_id)

print(f'Time taken to build the graph: {time.time() - start_time} seconds')

start_time = time.time()

# Compute the best partition using Girvan-Newman algorithm
comp = community.girvan_newman(graph)

# Get the first level communities (use next(comp) to get the next level)
community_generator = next(comp)

communities = [tuple(x) for x in community_generator]

with open('communitiesNX2.txt', 'w') as file:
    for community in communities:
        file.write('Community:\n')
        file.write(', '.join(map(str, community)))
        file.write('\n')
        file.write(f'Total nodes in community: {len(community)}\n\n')

print(f'Time taken to write communities to file: {time.time() - start_time} seconds')