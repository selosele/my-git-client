# my-git-client

Git 작업을 수행할 수 있는 심플한 GUI 클라이언트입니다.

## 작업 기간

- 2024.01.19 ~ 2024.03.01

## 개발 환경

- ```.NET``` - v8.0
- ```Avalonia``` - v11.0.7  
- ```LibGit2Sharp``` - v0.29.0
- ```Windows```에서 개발 및 구동 확인

## 빌드 및 실행

### 1. 프로젝트 빌드

```bash
dotnet build
```

### 2. 프로그램 실행

```bash
dotnet run
```

## 기능 목록

현재 구현된 기능 목록입니다:

- 로컬 Git 저장소 연결
- 최신 브랜치 조회
- 커밋, 풀, 푸시, 리셋 작업 수행
- 커밋 히스토리 확인
- 변경된 파일 확인
- Windows 탐색기에서 컨텍스트 메뉴로 프로그램 실행

## 라이센스

이 프로젝트는 MIT 라이센스 하에 배포됩니다.

## 스크린샷

![](https://res.cloudinary.com/dbrgfvqgb/image/upload/v1723079531/mygitclient_t87mmf.jpg)
