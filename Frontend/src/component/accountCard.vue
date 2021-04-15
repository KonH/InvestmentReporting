<template>
	<div class="card" style="width: 20rem">
		<div class="card-body">
			<h4>{{ account.displayName }}</h4>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { AccountDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'AccountCard',
	components: {},
})
export default class AccountCard extends Vue {
	@Prop()
	account!: AccountDto;
}
</script>
