import re
from collections import Counter

instances = Counter()
total = 0
cnt = 0
with open('communities/results/communitiesFollowing87k_instancesShow.json', 'r') as f:
    for line in f:
        match = re.findall('"instance": "([^"]*)"', line)
        if match:
            cnt += 1
            print(cnt)
            instances.update(match)
            total += len(match)

with open('communityTOP_2.txt', 'w') as f:
    f.write(f'Total unique instances: {len(instances)}\n')

    mastodon_count = instances['mastodon.social']
    f.write(f'Total mastodon.social instances: {mastodon_count}\n')

    non_mastodon_count = total - mastodon_count
    f.write(f'Total non-mastodon.social instances: {non_mastodon_count}\n')

    f.write('\nInstance Rankings:\n')

    for instance, count in instances.most_common():
        f.write(f'{instance}: {count}\n')
