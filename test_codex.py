from datetime import datetime

def main():
    now = datetime.now()
    print("Current date and time:", now.strftime("%Y-%m-%d %H:%M:%S"))

if __name__ == "__main__":
    main()
