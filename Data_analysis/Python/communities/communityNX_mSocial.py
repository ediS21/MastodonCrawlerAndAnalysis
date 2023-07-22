import networkx as nx
from community import community_louvain
import json
import time
from urllib.parse import urlparse

start_time = time.time()

# Read the data from the JSON file
data = []
with open('test2.json', 'r', encoding='utf-8') as file:
    for line in file:
        line = line.strip()
        if line:
            json_obj = json.loads(line)
            data.extend(json_obj)

# Create a directed graph
graph = nx.Graph()

node_instance_dict = {"None": "None"}

count = 0

# Add edges to graph
for item in data:
    count += 1
    print(count)
    source_id = str(item['source_id'])  # convert to string
    target_id = str(item['target_id'])  # convert to string
    profile_url = str(item['ProfileUrl'])

    parsed_uri = urlparse(profile_url)
    instance = '{uri.netloc}'.format(uri=parsed_uri)

    if source_id not in node_instance_dict:
        node_instance_dict[source_id] = "mastodon.social"

    if instance == 'mastodon.social':  # Check if target profile URL is from mastodon.social
        if target_id not in node_instance_dict:
            node_instance_dict[target_id] = instance

        if source_id and target_id and source_id != "None" and target_id != "None":
            graph.add_edge(source_id, target_id)

print(f'Time taken to build the graph: {time.time() - start_time} seconds')

start_time = time.time()

# Compute the best partition using Louvain algorithm
partition = community_louvain.best_partition(graph)

# Prepare communities list
communities = {}
for node, community in partition.items():
    if node_instance_dict[node] != "None":
        if community not in communities:
            communities[community] = []
        communities[community].append({"id": node, "instance": node_instance_dict[node]})

# Save communities to a file
with open('communitiesMSocial_test2.json', 'w') as file:
    for community, nodes in communities.items():
        # Only write communities with more than 100 nodes
        if len(nodes) > 1:
            file.write('Community:\n')
            json.dump(nodes, file)
            file.write('\n')
            file.write(f'Total nodes in community: {len(nodes)}\n\n')

print(f'Time taken to write communities to file: {time.time() - start_time} seconds')
