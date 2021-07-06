#!/bin/bash
# Script for building a unity project through the command-line with NBROS.BuildTools

#Global vars
GAME='MY_GAME'
UNITY_VERSION='2019.4.1f1'
PLATFORM='macOS'
START=$(date +"%s")
PLATFORMS[0]="MacOS"
PLATFORMS[1]="Windows64"
PLATFORMS[2]="Linux64"
UNITY="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
BUILD_METHOD="NBROS.Builds.BuildTools"

Main()
{
	MAIN_OPTIONS[0]="Build"
	MAIN_OPTIONS[1]="DisplayUnityInstallations"
	MAIN_OPTIONS[2]="ValidateCommandDirectory"
	MAIN_OPTIONS[3]='Quit'
	OPTION_INPUT=-1

	clear
	echo "$GAME Unity Command-line Build Tools"
	echo "------------------------------------"
	# echo "OS = $OSTYPE"

	while true; 
	do
		echo "Select Option:"
    		i=0
    		for O in ${MAIN_OPTIONS[@]}
    		do
			echo "  $i) $O"
			i=`expr $i + 1`
    		done

    		read OPTION_INPUT
    		case $OPTION_INPUT in
			[0]* ) BuildQuery;;
			[1]* ) DisplayUnityInstalls;;
			[2]* ) ValidateCommandDirectory;;
			[3]* ) exit;;
			[exit]* ) exit;;
			* ) echo "Invalid option.";;
    		esac
	done
}

ValidateCommandDirectory()
{
	echo 'This files directory is:'
	echo "$(dirname "$0")"

	cd "$(dirname "$0")"
	cd ..

	echo 'Unity project should be in the parent directory'
	echo ' '

	echo 'Targeted project directory is:'
	echo "$PWD/$GAME Project"

	# back to where we were
	cd "$(dirname "$0")"
	echo ' '
}


BuildQuery()
{
while true; do
    echo -e "Build $GAME for which platform?"
    i=0
    for P in ${PLATFORMS[*]}
    do
	echo "  $i) $P"
	i=`expr $i + 1`
    done

    read PLATFORM
    case $PLATFORM in
	[0]* ) PLATFORM="MacOS"; break;;
	[1]* ) PLATFORM="Windows64"; break;;
	[2]* ) PLATFORM="Linux64"; break;;
	[exit]* ) exit;;
	* ) echo "Invalid platform type.";;
    esac
done

while true; do
    read -p "Build $GAME for $PLATFORM, Unity v$UNITY_VERSION?" yn
    case $yn in
        [Yy]* ) Build; break;;
        [Nn]* ) break;;
	[exit]* ) exit;;
        * ) echo "Invalid response. Answer yes or no.";;
    esac

done
}

Build()
{
	start_time=`date +%s`

	echo "Beginning $GAME build process for $PLATFORM, Unity v$UNITY_VERSION..."

	cd "$(dirname "$0")"
	cd ..

	echo 'Active directory is:'
	echo $PWD

	echo "Building..."

	$UNITY -batchmode -logfile "Commands/Build Logs/$START build.log" -nographics -projectPath "$PWD/$GAME Project" -executeMethod "${BUILD_METHOD}.Build${PLATFORM}" -quit

	echo "Build Process Complete"
	end_time=`date +%s`
	echo execution time was `expr $end_time - $start_time` s.
}

DisplayUnityInstalls()
{
	echo 'Unity Installations:'
	ls -d /Applications/Unity/Hub/Editor/*/
	echo ' '
}

# Script start
clear
Main


