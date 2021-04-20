<template>
	<h1>Login</h1>
	<div class="form-group">
		<label>
			Username:
			<input ref="login" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Password:
			<input ref="password" type="password" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Login</button>
	<router-link to="/register" class="btn btn-secondary ml-2">Register</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Ref } from 'vue-property-decorator';
import { Action } from 'vuex-class';

@Options({
	name: 'LoginView',
})
export default class Login extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	@Ref('login')
	loginInput!: HTMLInputElement;

	@Ref('password')
	passwordInput!: HTMLInputElement;

	async onclick() {
		const loginResult = await Backend.tryFetch(
			Backend.auth().login.loginCreate({
				userName: this.loginInput.value,
				password: this.passwordInput.value,
			})
		);
		if (loginResult?.ok) {
			this.fetchState();
			await router.push('/');
		} else {
			alert('Login failed');
		}
	}
}
</script>
