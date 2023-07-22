import json
import networkx as nx

# Read the data from the 'who follows who' JSON file
follow_data = []
with open('test2.json', 'r', encoding='utf-8') as file:
    for line in file:
        line = line.strip()
        if line:
            json_obj = json.loads(line)
            follow_data.extend(json_obj)

# Create a directed graph from the 'who follows who' data
follow_graph = nx.DiGraph()
for relationship in follow_data:
    follow_graph.add_edge(relationship['source_id'], relationship['target_id'])

# Read the data from the communities JSON file and find strongly connected components
communities_data = []
with open('communitiesTest2.json', 'r', encoding='utf-8') as file:
    community = []
    for line in file:
        line = line.strip()
        if line.startswith('['):
            if community:  # If community is not empty, add it to communities_data
                communities_data.append(community)
            community = json.loads(line)  # Start a new community
        elif line.startswith(']'):
            community.append(json.loads(line))  # End of a community

if community:  # Add the last community if it wasn't added yet
    communities_data.append(community)

# Analyze each community
output = []
for community in communities_data:
    nodes_in_community = [node['id'] for node in community]

    # Create a subgraph for the current community
    community_graph = follow_graph.subgraph(nodes_in_community)

    # Find strongly connected components in the community graph
    strongly_connected_components = list(nx.weakly_connected_components(community_graph))

    # Analyze the components
    for component in strongly_connected_components:
        output.append({
            'community': nodes_in_community,
            'strongly_connected_component': list(component),
            'number_of_nodes': len(component)
        })

# Write the results to an output JSON file
with open('weakly_connected_Test2.json', 'w', encoding='utf-8') as file:
    json.dump(output, file, ensure_ascii=False)
