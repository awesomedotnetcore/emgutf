# --------------------------------------------------------
#  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.
# --------------------------------------------------------

IF(IS_UBUNTU AND HAVE_LINUX)
  
  PROJECT(runtime.ubuntu.Emgu.TF.nuget)
  #IF(WIN32)
  #ADD_CUSTOM_TARGET(
  #   ${PROJECT_NAME} ALL
  #   COMMAND "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
  #)
  #ELSE()
  #ADD_CUSTOM_TARGET(
  #   ${PROJECT_NAME} ALL
  #   COMMAND mono "${CMAKE_CURRENT_SOURCE_DIR}/../../../miscellaneous/nuget.exe" pack Package.nuspec -OutputDirectory "${CMAKE_CURRENT_SOURCE_DIR}/../"
  #)
  #ENDIF()
  ADD_CUSTOM_TARGET(
    ${PROJECT_NAME} ALL
    COMMAND ${DOTNET_EXECUTABLE} pack -c ${DEFAULT_CS_CONFIG} "${CMAKE_CURRENT_SOURCE_DIR}/../../../Emgu.TF/Emgu.TF.Netstandard/Emgu.TF.Netstandard.csproj" -p:NuspecFile="${CMAKE_CURRENT_SOURCE_DIR}/Package.nuspec" -o "${CMAKE_CURRENT_SOURCE_DIR}/../"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    )
  
  ADD_DEPENDENCIES(${PROJECT_NAME} Emgu.TF.nuget)
  
  if(ENABLE_SOLUTION_FOLDERS)
    set_target_properties(${PROJECT_NAME} PROPERTIES FOLDER "NUGET")
  endif() 
  
ENDIF()
