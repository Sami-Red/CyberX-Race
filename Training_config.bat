@echo off
cd /d D:\UnityProjects\Final_Year_Project

mlagents-learn trainer_config.yaml --run-id=RacerAI_1M_Run --env Builds\RacerTrainer.exe --resume --time-scale=10 --no-graphics --debug

pause
