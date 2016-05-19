msbuild=msbuild.exe /m /verbosity:m /nologo
nuget=nuget.exe

.PHONY: install
install: build-release
	cp AutoFluent.Command/bin/Release/{*.dll,*.exe,*.config} /usr/local/bin/

.PHONY: build-release
build-release: conf=Release
build-release: build

.PHONY: build
build:
	${msbuild} AutoFluent.sln /p:Configuration=${conf} 
	
.PHONY: update-nuget
update-nuget:
	rm -f /usr/local/bin/nuget.exe
	cd /usr/local/bin && wget https://www.nuget.org/nuget.exe && chmod +x nuget.exe

# Xamarin.Forms.AutoFluent

# we want to always stay below the current xamarin forms version to avoid
# confusion.

xfa-ver=2.2.0.1

.PHONY: package-xfa
package-xfa: ver=${xfa-ver}
package-xfa: name=Xamarin.Forms.AutoFluent
package-xfa: conf=Release
package-xfa: build
	cd ${name} && ${nuget} pack ${name}.csproj -Version ${ver} -Prop Configuration=${conf}

.PHONY: distribute-xfa
distribute-xfa: ver=${xfa-ver}
distribute-xfa: name=Xamarin.Forms.AutoFluent
distribute-xfa: package-xfa
	cd ${name} && nuget push ${name}.${ver}.nupkg	



