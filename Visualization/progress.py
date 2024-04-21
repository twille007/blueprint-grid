import os
import matplotlib.pyplot as plt

def read_file(file_path):
    with open(file_path, 'r') as file:
        average_rewards = [float(line.strip().replace(',', '.')) for line in file]
    return average_rewards
    
def plot_content(content, title, ax):
    ax.plot(content)
    ax.set_title(title)
    ax.set_xlabel('Episode')
    ax.set_ylabel('Value')
    ax.grid(True)
    
def main():
    file_path = "../GridBlueprint/Resources/avg_rewards.txt"
    if not os.path.isfile(file_path):
        print("File not found:", file_path)
        return
    average_rewards = read_file(file_path)

    file_path = "../GridBlueprint/Resources/steps.txt"
    if not os.path.isfile(file_path):
        print("File not found:", file_path)
        return
    steps = read_file(file_path)
    
    fig, axs = plt.subplots(1, 2, figsize=(12, 5))
    
    plot_content(average_rewards, 'Average Rewards', axs[0])
    plot_content(steps, 'Steps', axs[1])

    plt.tight_layout()
    plt.show()

if __name__ == "__main__":
    main()
